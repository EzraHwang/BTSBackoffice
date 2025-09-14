# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BTS Backoffice is a .NET 8 ticketing management system backend platform that integrates with N8N workflows. The system manages ticket sales for two types of tickets: venue tickets (amusement parks, museums) and transportation tickets. All data is retrieved through N8N API calls that connect to Google Sheets - there are no direct database connections.

## Architecture

### Data Flow Architecture
- **No Direct Database**: All data operations go through N8N API calls
- **N8N Integration**: N8N serves as middleware connecting to Google Sheets backend
- **API-First Design**: Backend communicates with N8N via HTTP APIs
- **Email Processing**: N8N handles email checking and content extraction for vendor responses

### Core System Components
1. **AAA System**: Authentication, Authorization, and Account management for admin users
2. **Ticket Management**: Manual ticket issuance and ticket status management
3. **Order Management**: View and manage user purchase information and ticket details
4. **External API Integration**: Stripe API integration for payment processing and financial reports

### N8N Configuration
- N8N domain should be configurable via settings/config to support different environments
- Backend routes remain consistent across environments, only domain changes
- All data retrieval APIs point to N8N endpoints that fetch from Google Sheets

## Development Commands

### Build and Run
```bash
# Navigate to web project
cd BTSBackoffice.Web

# Build the project
dotnet build

# Run in development mode (default port: 5199)
dotnet run

# Run with specific environment
dotnet run --environment Development

# Build and run from solution root
dotnet build BTSBackoffice.sln
```

### Testing
```bash
# Run all tests (when test projects are added)
dotnet test

# Run specific test project
dotnet test Tests/BTSBackoffice.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Package Management
```bash
# Restore packages
dotnet restore

# Add package to web project
dotnet add BTSBackoffice.Web package PackageName

# Update packages
dotnet list BTSBackoffice.Web package --outdated
```

### Application URLs
- Development: http://localhost:5199
- Login page: http://localhost:5199/login
- Dashboard: http://localhost:5199/dashboard

## Key Integration Points

### N8N API Integration
- Configure N8N base URL in appsettings
- All CRUD operations route through N8N workflows
- Email processing workflows for vendor communication
- Google Sheets data synchronization

### External APIs
- **Stripe Integration**: Payment processing and financial reporting
- **Email Integration**: Vendor communication for ticket approval workflow

### Ticket Workflow
1. Users purchase tickets via mobile app (external system)
2. Venue tickets trigger email to vendors
3. Backend monitors vendor email responses via N8N
4. Admin can manually issue tickets through backoffice
5. Ticket delivery to purchasers

## Admin Features
- User role and permission management
- Manual ticket issuance controls
- Order and ticket information dashboard
- Financial report access via Stripe integration

## Configuration Management
- Environment-specific N8N domain configuration
- API endpoint configuration for external services
- Authentication and authorization settings
- Email processing workflow configuration

## Implementation Status

### Completed Features (Flow A)
✅ **Authentication System**
- Session-based login with configurable admin credentials
- Login attempt tracking (5 attempts = 15-minute lockout)
- Secure session management with proper cookie settings

✅ **Dashboard KPIs**
- Total Revenue, Tickets Sold, Refunds, Net Revenue
- To Settle Vendors, To Payout Stripe
- Real-time data with skeleton loading effects

✅ **Statistical Charts**
- Line chart: Daily revenue trends
- Bar chart: Daily ticket sales
- Pie chart: Ticket type distribution (venue vs transport)

✅ **Time Range Filtering**
- Today, 7 days, 30 days, custom date range
- AJAX-based updates without page reload
- GMT+8 timezone support

✅ **N8N Integration**
- Configurable API service with HttpClient
- Memory caching (1-minute expiration)
- Error handling with user-friendly messages
- Mock data implementation ready for N8N endpoints

### Configuration Files
- `appsettings.json`: Contains admin credentials, N8N settings, Stripe config
- Default login: admin/Admin123!
- N8N BaseUrl: configurable for different environments

### API Endpoints Implemented
- `POST /login` - Authentication
- `GET /logout` - Session cleanup
- `GET /dashboard` - Dashboard page
- `GET /api/dashboard` - Dashboard data API (AJAX)