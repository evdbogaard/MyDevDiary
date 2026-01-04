# Base Image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base

# Common packages for all variants
RUN apt-get update && apt-get install -y \
    curl \
    git \
    vim \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Image for devcontainer
FROM base AS dev
### Figure out dotnet workload update issue
ARG USERNAME=dev
ARG USER_UID=1337
ARG USER_GID=$USER_UID

RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && apt-get update \
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

RUN apt-get clean && rm -rf /var/lib/apt/lists/*
USER $USERNAME
# CMD ["bash"]

# Image for CI/CD pipeline
FROM base AS ci
