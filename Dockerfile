FROM node:18-alpine

# Install system dependencies
RUN apk add --no-cache python3 make g++ git

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Expose ports
EXPOSE 3000 9090

# Set environment variables
ENV NODE_ENV=production

# Start the service
CMD ["npm", "start"]