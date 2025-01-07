#!/bin/bash

/Applications/Unity/Hub/Editor/6000.0.28f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -executeMethod SanityCheck.RunSanityCheck -logFile sanity.log -quit
