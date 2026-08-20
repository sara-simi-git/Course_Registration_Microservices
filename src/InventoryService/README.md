# InventoryService

InventoryService is a dedicated microservice for managing course capacity.

## Responsibilities
- Create and manage course inventory records
- Track max capacity and reserved seats
- Reserve and release seats for a course

## Database
- PostgreSQL
- Entity Framework Core migrations

## Endpoints
- GET /api/inventory
- GET /api/inventory/course/{courseId}
- POST /api/inventory
- PUT /api/inventory/capacity
- POST /api/inventory/reserve
- POST /api/inventory/release
- DELETE /api/inventory/{id}
