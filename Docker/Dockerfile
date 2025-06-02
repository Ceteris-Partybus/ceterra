# Use the Unity Multiplay Linux base image
FROM unitymultiplay/linux-base-image:latest

# Set working directory inside the container
WORKDIR /app

# Copy the built Unity Linux headless server executable and any required files
# Assuming the build context is the 'Docker' directory, and the build output is in '../Build/Linux/Server/'
COPY ../Build/Linux/Server/ ./

# Make the server executable (assuming its name is DedicatedServer based on BuildScript.cs)
RUN chmod +x DedicatedServer

# Expose the port your server listens on (replace 7777 with your actual port if different)
EXPOSE 7777/udp
EXPOSE 7777/tcp

# Run the server executable from the /app directory
CMD ["/app/DedicatedServer", "-batchmode", "-nographics", "-logFile", "/dev/stdout"]
