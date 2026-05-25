FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

ARG VITE_GOOGLE_CLIENT_ID=""
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID

RUN npm run build

FROM nginx:alpine AS runtime

# The official nginx:alpine image processes /etc/nginx/templates/*.template
# through envsubst on startup, writing the result to /etc/nginx/conf.d/.
# We restrict substitution to ${API_UPSTREAM} so nginx's own $-vars
# (like $http_upgrade, $host) pass through untouched.
ENV NGINX_ENVSUBST_TEMPLATE_SUFFIX=.template \
    NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx/conf.d \
    NGINX_ENVSUBST_FILTER='^API_UPSTREAM(_HOST)?$' \
    API_UPSTREAM=http://api:8080 \
    API_UPSTREAM_HOST=api

RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget -q -O /dev/null http://localhost/ || exit 1
