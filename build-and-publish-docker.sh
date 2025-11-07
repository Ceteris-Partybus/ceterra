#!/bin/bash

# on the remote server, then execute the following commands to pull the image
# echo "<PAT>" | docker login ghcr.io -u <github_username> --password-stdin
# docker pull ghcr.io/ceteris-partybus/ceterra/ceterra:<tag>

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
DEFAULT_IMAGE_NAME="ceterra"
DEFAULT_TAG="latest"

# Try to get GitHub username from various sources
if [ -n "${GITHUB_ACTOR:-}" ]; then
    DEFAULT_GITHUB_ACTOR="$GITHUB_ACTOR"
elif command -v git &> /dev/null; then
    # Try git config
    DEFAULT_GITHUB_ACTOR=$(git config --get user.name 2>/dev/null || echo "")
else
    DEFAULT_GITHUB_ACTOR=""
fi

DEFAULT_GITHUB_REPOSITORY="${GITHUB_REPOSITORY:-ceteris-partybus/ceterra}"

# Try to load GITHUB_PAT from .env file
if [ -f ".env" ]; then
    source .env
    DEFAULT_CR_PAT="${GITHUB_PAT:-}"
else
    DEFAULT_CR_PAT="${CR_PAT:-}"
fi

# Print header
echo -e "${CYAN}╔═══════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║     Docker Image Build & Push to GHCR Utility        ║${NC}"
echo -e "${CYAN}╚═══════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}ERROR: Docker is not installed or not in PATH.${NC}"
    echo -e "${RED}Please install Docker before running this script.${NC}"
    exit 1
fi
echo -e "${GREEN}[OK] Docker is installed${NC}"
echo ""

# Check if git is installed and verify branch
if command -v git &> /dev/null; then
    CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
    
    if [ "$CURRENT_BRANCH" != "main" ]; then
        echo -e "${RED}ERROR: Not on main branch!${NC}"
        echo -e "${RED}Current branch: ${CURRENT_BRANCH}${NC}"
        echo -e "${RED}This script should only be run from the main branch.${NC}"
        exit 1
    else
        echo -e "${GREEN}[OK] Git: On main branch${NC}"
    fi
else
    echo -e "${YELLOW}WARNING: Git is not installed or not in PATH.${NC}"
    echo -e "${YELLOW}Cannot verify that you are on the correct branch.${NC}"
    echo -e "${YELLOW}Please ensure you are on the main branch manually.${NC}"
fi
echo ""

# Important reminder about build
echo -e "${BLUE}IMPORTANT: Please ensure your latest build is already built${NC}"
echo -e "${BLUE}into the correct folder (Builds/Server) before proceeding!${NC}"
echo ""

# Reminder and link to PAT creation
echo -e "${YELLOW}NOTE: You will need a GitHub Personal Access Token (PAT) with 'write:packages' and 'read:packages' scope to push images.${NC}"
echo -e "${YELLOW}You can create a PAT here: https://github.com/settings/tokens/new?scopes=write:packages,read:packages${NC}"
echo -e "${RED}Make sure to store it securely as you won't be able to see it again!${NC}"
if [ ! -f ".env" ]; then
    echo -e "${YELLOW}TIP: You can create a .env file (see .env.example) and add your PAT as GITHUB_PAT to avoid entering it each time.${NC}"
fi
echo ""

# Prompt for inputs
echo -e "${BLUE}Please provide the following information:${NC}"
echo ""

# Image name
read -p "$(echo -e ${CYAN}Image name ${NC}[${DEFAULT_IMAGE_NAME}]: )" IMAGE_NAME
IMAGE_NAME="${IMAGE_NAME:-$DEFAULT_IMAGE_NAME}"

# Tag
read -p "$(echo -e ${CYAN}Tag ${NC}[${DEFAULT_TAG}]: )" TAG
TAG="${TAG:-$DEFAULT_TAG}"

# GitHub Actor
read -p "$(echo -e ${CYAN}GitHub username ${NC}[${DEFAULT_GITHUB_ACTOR}]: )" GITHUB_ACTOR_INPUT
GITHUB_ACTOR_INPUT="${GITHUB_ACTOR_INPUT:-$DEFAULT_GITHUB_ACTOR}"

if [ -z "$GITHUB_ACTOR_INPUT" ]; then
    echo -e "${RED}ERROR: GitHub username is required${NC}"
    exit 1
fi

# GitHub Repository
read -p "$(echo -e ${CYAN}GitHub repository ${NC}[${DEFAULT_GITHUB_REPOSITORY}]: )" GITHUB_REPOSITORY_INPUT
GITHUB_REPOSITORY_INPUT="${GITHUB_REPOSITORY_INPUT:-$DEFAULT_GITHUB_REPOSITORY}"

if [ -z "$GITHUB_REPOSITORY_INPUT" ]; then
    echo -e "${RED}ERROR: GitHub repository is required${NC}"
    exit 1
fi

# Personal Access Token
if [ -z "$DEFAULT_CR_PAT" ]; then
    read -sp "$(echo -e ${CYAN}GitHub PAT ${NC}[required]: )" CR_PAT_INPUT
    echo ""
else
    echo -e "${GREEN}[OK] Using GITHUB_PAT from .env file${NC}"
    read -sp "$(echo -e ${CYAN}GitHub PAT ${NC}[press Enter to use .env, or enter a different PAT]: )" CR_PAT_INPUT
    echo ""
    CR_PAT_INPUT="${CR_PAT_INPUT:-$DEFAULT_CR_PAT}"
fi

if [ -z "$CR_PAT_INPUT" ]; then
    echo -e "${RED}ERROR: GitHub Personal Access Token is required${NC}"
    exit 1
fi

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}Configuration Summary:${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo -e "  Image Name:    ${GREEN}${IMAGE_NAME}${NC}"
echo -e "  Tag:           ${GREEN}${TAG}${NC}"
echo -e "  GitHub User:   ${GREEN}${GITHUB_ACTOR_INPUT}${NC}"
echo -e "  Repository:    ${GREEN}${GITHUB_REPOSITORY_INPUT}${NC}"
echo -e "  Registry:      ${GREEN}ghcr.io${NC}"
echo -e "  Full Image:    ${GREEN}ghcr.io/${GITHUB_REPOSITORY_INPUT}/${IMAGE_NAME}:${TAG}${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo ""

# Confirm before proceeding
read -p "$(echo -e ${YELLOW}Proceed with build and push? ${NC}[y/N]: )" CONFIRM
if [[ ! "$CONFIRM" =~ ^[Yy]$ ]]; then
    echo -e "${RED}Aborted by user${NC}"
    exit 0
fi

echo ""
echo -e "${CYAN}Starting Docker operations...${NC}"
echo ""

# Build the Docker image
echo -e "${BLUE}[1/4]${NC} Building Docker image..."
if docker build -f Dockerfile.ghcr -t "${IMAGE_NAME}:${TAG}" .; then
    echo -e "${GREEN}[OK] Build successful${NC}"
else
    echo -e "${RED}[FAILED] Build failed${NC}"
    exit 1
fi
echo ""

# Log in to GitHub Container Registry
echo -e "${BLUE}[2/4]${NC} Logging in to GitHub Container Registry..."
if echo "$CR_PAT_INPUT" | docker login ghcr.io -u "$GITHUB_ACTOR_INPUT" --password-stdin; then
    echo -e "${GREEN}[OK] Login successful${NC}"
else
    echo -e "${RED}[FAILED] Login failed${NC}"
    exit 1
fi
echo ""

# Tag the image
echo -e "${BLUE}[3/4]${NC} Tagging image..."
FULL_IMAGE_NAME="ghcr.io/${GITHUB_REPOSITORY_INPUT}/${IMAGE_NAME}:${TAG}"
if docker tag "${IMAGE_NAME}:${TAG}" "$FULL_IMAGE_NAME"; then
    echo -e "${GREEN}[OK] Tag successful${NC}"
else
    echo -e "${RED}[FAILED] Tagging failed${NC}"
    exit 1
fi
echo ""

# Push the image
echo -e "${BLUE}[4/4]${NC} Pushing image to registry..."
if docker push "$FULL_IMAGE_NAME"; then
    echo -e "${GREEN}[OK] Push successful${NC}"
else
    echo -e "${RED}[FAILED] Push failed${NC}"
    exit 1
fi
echo ""

echo -e "${GREEN}╔═══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║            All operations completed!                  ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "Image successfully pushed to: ${CYAN}${FULL_IMAGE_NAME}${NC}"
echo ""