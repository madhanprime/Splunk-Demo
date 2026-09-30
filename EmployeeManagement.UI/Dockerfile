# ========================================================
# Angular 21 UI Dockerfile for Render / Cloud Deployment
# ========================================================

# Stage 1: Build Angular application
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . ./
RUN npx ng build --configuration production

# Stage 2: Serve via Nginx
FROM nginx:alpine AS final

# Install curl for health checks
RUN apk add --no-cache curl

# Render default port
ENV PORT=10000

# Copy built Angular files to Nginx html directory
COPY --from=build /app/dist/EmployeeManagement.UI/browser /usr/share/nginx/html

# Copy dynamic nginx template (Render $PORT substitution)
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 10000

CMD ["nginx", "-g", "daemon off;"]
