# WAPS Reverse Proxy Deployment Guide

## Overview
This guide explains how to deploy WAPS behind an Nginx reverse proxy with SSL/TLS support for production use.

## Prerequisites
- Linux server (Ubuntu/Debian recommended)
- Nginx installed (`sudo apt install nginx`)
- .NET 8.0 Runtime installed
- Domain name pointing to your server
- SSL certificate (Let's Encrypt recommended)

## Architecture

```
[Mobile Devices] 
      ↓ HTTPS (443)
[Nginx Reverse Proxy with SSL]
      ↓ HTTP (localhost:5278)
[ASP.NET Core WAPS Application]
      ↓
[SQLite + SQL Server Databases]
```

## Step 1: Install SSL Certificate

### Option A: Let's Encrypt (Recommended - Free)
```bash
# Install certbot
sudo apt install certbot python3-certbot-nginx

# Get certificate (interactive)
sudo certbot --nginx -d waps.yourdomain.com

# Auto-renewal is configured automatically
```

### Option B: Custom Certificate
```bash
# Place your certificate files:
sudo cp your-cert.crt /etc/ssl/certs/waps.crt
sudo cp your-key.key /etc/ssl/private/waps.key
sudo chmod 600 /etc/ssl/private/waps.key
```

## Step 2: Configure Nginx

```bash
# Copy the example configuration
sudo cp nginx.conf.example /etc/nginx/sites-available/waps

# Edit the configuration
sudo nano /etc/nginx/sites-available/waps

# Update these values:
# - server_name: your actual domain
# - ssl_certificate paths (if not using Let's Encrypt)
# - proxy_pass port (if ASP.NET Core uses different port)

# Enable the site
sudo ln -s /etc/nginx/sites-available/waps /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx
```

## Step 3: Configure ASP.NET Core Application

The application is already configured to support reverse proxy with:
- Forwarded headers middleware
- Proper WebSocket support for SignalR
- CORS configured

### Update appsettings.json for production:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "waps.yourdomain.com",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=dbMillionaire;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

## Step 4: Run ASP.NET Core as SystemD Service

Create service file:
```bash
sudo nano /etc/systemd/system/waps.service
```

Service file content:
```ini
[Unit]
Description=WAPS - Web-Based Audience Participation System
After=network.target

[Service]
WorkingDirectory=/var/www/waps
ExecStart=/usr/bin/dotnet /var/www/waps/MillionaireGame.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=waps
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5278

[Install]
WantedBy=multi-user.target
```

Enable and start service:
```bash
# Deploy your application
sudo mkdir -p /var/www/waps
sudo cp -r /path/to/published/app/* /var/www/waps/
sudo chown -R www-data:www-data /var/www/waps

# Enable service
sudo systemctl enable waps
sudo systemctl start waps
sudo systemctl status waps
```

## Step 5: Firewall Configuration

```bash
# Allow HTTP and HTTPS
sudo ufw allow 'Nginx Full'

# Or manually:
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Enable firewall
sudo ufw enable
sudo ufw status
```

## Step 6: Dedicated WiFi Network Setup

### Option A: Captive Portal
Configure your WiFi access point to redirect all traffic to `https://waps.yourdomain.com`

### Option B: Custom DNS
1. Configure DHCP server to provide custom DNS
2. Set DNS records to point `waps.local` → Server IP
3. Participants connect to WiFi and navigate to `https://waps.local`

### Option C: Simple Join Instructions
1. Display QR code with URL: `https://waps.yourdomain.com`
2. Participants scan QR code to join
3. Browser opens directly to HTTPS URL

## Troubleshooting

### WebSocket Connection Fails
Check Nginx logs:
```bash
sudo tail -f /var/log/nginx/waps_error.log
```

Verify WebSocket upgrade headers are being sent:
```bash
curl -i -N -H "Connection: Upgrade" -H "Upgrade: websocket" https://waps.yourdomain.com/hubs/fff
```

### SSL Certificate Issues
Verify certificate:
```bash
sudo certbot certificates
```

Renew manually:
```bash
sudo certbot renew --dry-run
```

### Application Not Responding
Check service status:
```bash
sudo systemctl status waps
sudo journalctl -u waps -f
```

Check if port 5278 is listening:
```bash
sudo netstat -tlnp | grep 5278
```

### CORS Errors
Update CORS policy in Program.cs to whitelist your domain:
```csharp
policy.WithOrigins("https://waps.yourdomain.com")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
```

## Performance Tuning

### Nginx Worker Processes
```nginx
# In /etc/nginx/nginx.conf
worker_processes auto;
worker_connections 4096;
```

### ASP.NET Core
```bash
# Increase file descriptor limits
ulimit -n 65536
```

### Database Optimization
- Regular SQLite VACUUM for waps.db
- Ensure SQL Server has proper indexes
- Monitor connection pool usage

## Security Considerations

1. **Rate Limiting**: Nginx config includes rate limiting (10 req/s general, 100 req/s API)
2. **SSL/TLS**: Modern cipher suites, TLS 1.2+ only
3. **Headers**: Security headers configured (HSTS, X-Frame-Options, etc.)
4. **Profanity Filter**: Implemented in name validation
5. **Input Validation**: 35 character limit, no emojis, trimmed whitespace

## Monitoring

### Check Application Health
```bash
curl https://waps.yourdomain.com/health
```

### Monitor Nginx Access
```bash
sudo tail -f /var/log/nginx/waps_access.log
```

### Monitor Application Logs
```bash
sudo journalctl -u waps -f
```

## Backup

### Database Backup
```bash
# Backup SQLite
cp /var/www/waps/waps.db /backup/waps.db.$(date +%Y%m%d)

# Export game statistics (automatic at game end)
# Files saved to: /var/www/waps/statistics/
```

### Configuration Backup
```bash
# Backup Nginx config
sudo cp /etc/nginx/sites-available/waps /backup/waps-nginx.conf

# Backup service file
sudo cp /etc/systemd/system/waps.service /backup/
```

## Production Checklist

- [ ] SSL certificate installed and valid
- [ ] Nginx configuration updated with correct domain
- [ ] ASP.NET Core service running
- [ ] Firewall rules configured
- [ ] Health endpoint responding
- [ ] WebSocket connections working
- [ ] QR code displays correctly
- [ ] Mobile devices can connect
- [ ] Statistics export working
- [ ] Backups configured
- [ ] Monitoring in place

## Support

For issues related to:
- **Nginx**: Check `/var/log/nginx/waps_error.log`
- **ASP.NET Core**: Check `sudo journalctl -u waps -f`
- **SSL**: Run `sudo certbot certificates`
- **WebSockets**: Verify Nginx upgrade headers configuration

## Updates

To update the application:
```bash
# Stop service
sudo systemctl stop waps

# Deploy new version
sudo cp -r /path/to/new/published/app/* /var/www/waps/

# Start service
sudo systemctl start waps

# Verify
sudo systemctl status waps
curl https://waps.yourdomain.com/health
```
