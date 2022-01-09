# OneClickDesktop Overseer

## Certificate

Needs to be in `.pfx` format with password.

## Run inside container

Remember to  map ports and set volumes for configuration and certificate.

Example command:

```
docker run --name overseer -d -p 80:5000 -p 443:5001 -v $PWD/Overseer/config:/overseer/config -v $PWD/overseer.pfx:/overseer/overseer.pfx one-click-desktop/overseer
```