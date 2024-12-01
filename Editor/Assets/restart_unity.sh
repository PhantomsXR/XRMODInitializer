#!/bin/bash

# Unity Editor Path
UNITY_PATH=$1

# Current Project Path
PROJECT_PATH=$2

# Close Unity Editor
# osascript -e 'tell application "Unity" to quit'

# Wait until the Unity editor has closed
sleep 2

# Restart Unity Editor
"$UNITY_PATH" --args -projectPath "$PROJECT_PATH"