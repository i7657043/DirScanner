# Dir Scanner

Simple overview of use/purpose.

## Description

A simple .NET Core console application to list the largest directories at a given path

## Getting Started

### Dependencies

* Windows OS

### Installing

* No installation is required

### Executing program

You can either execute the application by double clicking the dirscanner.exe file

Or by running from the command line
```
.\dirscanner.exe
```
You can also use the following flags
```
.\dirscanner.exe -p|--path C:\ -s-|-sizeout 100 -v|--verbose
```
* `-p|--path` (string) the path to scan from
* `-s|-sizeout` (number) the number of directories to show in output
* `-v|--verbose` (no value) include verbose output