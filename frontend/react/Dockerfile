FROM node:22.16.0 AS build
ARG VITE_ENV=development
WORKDIR /app
COPY package*.json ./
COPY package-lock.json ./
RUN npm install
COPY . .
RUN npm run build

# Use official nginx image for production
FROM nginx:1.25-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf nginx.conf

HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:80 || exit 1

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
