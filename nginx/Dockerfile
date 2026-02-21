FROM nginx:latest
COPY nginx.conf /etc/nginx/nginx.conf
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/ssl/private/nginx.key -out /etc/ssl/certs/nginx.crt -subj "/CN=localhost"
CMD ["nginx", "-g", "daemon off;"]