#!/bin/bash

if [ "$EUID" -ne 0 ]
then
	echo "Run this script as root like this: sudo ./SHServiceManager.sh"
	exit
fi

HOME_FOLDER=/home/pi
SH_FOLDER=$HOME_FOLDER/IBT_2022
DOTNET=$HOME_FOLDER/.dotnet/dotnet

compileSH()
{
	printf "\nCompiling SmartHeater Hub software...\n"
	$DOTNET publish -c Release $SH_FOLDER/src/SmartHeater.Hub/SmartHeater.Hub.csproj
}

startSH()
{
	printf "\nStarting and enabling the service on boot...\n"
	systemctl enable SmartHeater.service
	systemctl start SmartHeater.service
}

stopSH()
{
	printf "\nStopping and disabling the service...\n"
	systemctl stop SmartHeater.service
	systemctl disable SmartHeater.service
}

pullFromGit()
{
	printf "\nPulling from git repository...\n"
	git pull
}

# Copy only if service file does not exist already or is updated.
cp --update $SH_FOLDER/SmartHeater.service /etc/systemd/system/

echo "What do you want to do with SmartHeater Hub service? (start/stop/update)"
read action

if [[ $action == "start" ]]
then
    # Compile C# project if needed.
    if [[ ! -f $SH_FOLDER/src/SmartHeater.Hub/bin/Release/net6.0/publish/SmartHeater.Hub.dll ]]
    then
        compileSH
	else
		echo "SmartHeater.Hub project is already compiled. Recompile? (y/n)"
		read compile
		if [[ $compile == "y" ]]
		then
			compileSH
		fi	
	fi
	startSH

elif [[ $action == "stop" ]]
then
	stopSH

elif [[ $action == "update" ]]
then
	stopSH
	pullFromGit
	compileSH
	startSH

else
	echo "Invalid input!"
	echo "Available commands: start, stop"
fi
