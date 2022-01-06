#! /bin/bash

docker build . -t one-click-desktop/overseer

# On run remember to:
# 1. Pass port to exposed 5000 and 5001