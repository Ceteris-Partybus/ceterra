# Start from Unity's Game Server Hosting base image
FROM unitymultiplay/linux-base-image:ubuntu-22.04

# Switch to root to set up permissions
USER root

# Set up the game directory
WORKDIR /game
COPY --chown=mpukgame:mpukgame ./Builds/LinuxServer/ .
RUN chmod +x ./Server

# Switch back to the game user
USER mpukgame

# Expose ports for multiplayer (UDP and TCP)
EXPOSE 7777/udp
EXPOSE 7777/tcp

# Set the entrypoint to the server executable with fixed parameters
ENTRYPOINT ["./Server", "-nographics", "-headless", "-logFile", "/dev/stdout", "-batchmode"]

# Default parameters that can be overridden
CMD ["-serverIP", "0.0.0.0", "-port", "7777"]