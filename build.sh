#! /bin/bash

docker build . -t one-click-desktop/overseer

# On run remember to:
# 1. Pass port to exposed 5000 and 5001
# 2. Pass volume containing configs (to /overseer/config/)
# 3. Pass volume containing certificate (to path specified in config)