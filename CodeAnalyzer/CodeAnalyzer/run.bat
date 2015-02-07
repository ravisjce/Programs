:run.bat
@echo off

@echo =========================================================
@echo Demonstrating the functionalities of Code Analyser Tool
@echo =========================================================

@echo Testing the tool for the current directory
cd Executive/bin/Debug/
Executive.exe . "*.cs" 

@echo Testing the tool for a directory input with recursive definition
Executive.exe ..//..//..//Test "*.cs" "/S" 

@echo Testing the tool with a directory input and with relationships definition
Executive.exe . "*.cs" "/S" "/R"

@echo Testing the tool with a directory input, with relationships definition and xml generation
Executive.exe . "*.cs" "/R" "/X"

cd ../../..
