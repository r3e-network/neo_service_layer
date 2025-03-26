# Build stage
FROM golang:1.21-alpine AS builder

# Install build dependencies
RUN apk add --no-cache git make

# Set working directory
WORKDIR /app

# Copy go mod and sum files
COPY go.mod go.sum ./

# Download dependencies
RUN go mod download

# Copy source code
COPY . .

# Build the application
RUN make build

# Final stage
FROM alpine:latest

# Install runtime dependencies
RUN apk add --no-cache ca-certificates tzdata

# Set working directory
WORKDIR /app

# Copy binary from builder
COPY --from=builder /app/bin/neo-service-layer .
COPY --from=builder /app/bin/cli .

# Copy config file
COPY config.yaml .

# Expose port
EXPOSE 8080

# Set environment variables
ENV CONFIG_FILE=/app/config.yaml

# Run the application
CMD ["./neo-service-layer", "--config", "config.yaml"]