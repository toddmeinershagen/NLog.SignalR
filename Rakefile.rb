#!/usr/bin/env ruby

require 'albacore'
require 'fileutils'

CONFIG = 'Release'
RAKE_DIR = File.expand_path(File.dirname(__FILE__))
SOLUTION_DIR = RAKE_DIR + '/src'
SOLUTION_FILE = 'NLog.SignalR.sln'
NUGET = SOLUTION_DIR + "/.nuget/nuget.exe"

task :default => ['build']
# task :test => ['mstest' ]
task :package => ['package:packall']
task :push => ['package:pushall']

build :build do |b|
  b.sln  = "#{SOLUTION_DIR}/#{SOLUTION_FILE}"
  b.target = ['Clean', 'Rebuild']
  b.prop 'Configuration', CONFIG
  b.logging = 'quiet'
end

namespace :package do

	def create_packs()
		create_pack 'NLog.Signalr'
	end

	def create_pack(name)
		sh NUGET + " pack #{SOLUTION_DIR}/#{name}/#{name}.csproj -o pack"
	end

	task :packall => [ :clean, 'build' ] do
		Dir.mkdir('pack')
		create_packs	
		Dir.glob('pack/*') { |file| FileUtils.move(file,'nuget/') }
		Dir.rmdir('pack')
	end

	task :pushall => [ :clean, 'build' ] do

		puts "Please enter the project's NuGet API Key:"
		key = STDIN.gets.strip
		sh NUGET + ' setApiKey ' + key

		Dir.mkdir('pack')
		create_packs	
		Dir.chdir('pack')
		Dir.glob('*').each do |file|
			sh NUGET + ' push ' + file
			FileUtils.move(file,'../nuget/')
		end
		Dir.chdir('..')
		Dir.rmdir('pack')
	end

	task :clean do
		if Dir.exists? 'pack'
			FileUtils.remove_dir 'pack', force = true
		end
	end
end