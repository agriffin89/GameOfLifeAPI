🟢 Game of Life API

📌 Problem Description

The Game of Life API is an implementation of Conway's Game of Life, a zero-player cellular automaton. The game consists of a grid of cells that evolve through iterations based on predefined rules.

This API provides:

State persistence: Saves and retrieves board states.

Next state calculation: Computes the next generation of the board.

Final state detection: Determines if the board reaches a stable state.

🚀 Steps to Run the Code Locally

✅ Prerequisites

Ensure you have the following installed:

.NET 7 SDK → Download Here

Git

SQLite (optional, only if modifying database manually)

Docker (optional, for containerization)

🛠️ Step 1: Clone the Repository

git clone https://github.com/agriffin89/GameOfLifeAPI.git
cd GameOfLifeAPI

🔗 Step 2: Install Dependencies

dotnet restore

(If dotnet ef is not installed, run this command first):

dotnet tool install --global dotnet-ef

📌 Step 3: Run Database Migrations

dotnet ef migrations add InitialCreate
dotnet ef database update

▶️ Step 4: Run the API Locally

dotnet run --project GameOfLifeAPI

The API will be running at:

API Base URL → http://localhost:5000

Swagger UI → http://localhost:5000/swagger

📡 Step 5: Test the API Endpoints

You can use cURL, Postman, or Swagger UI to test the API.

🔹 Save a Board State

curl -X POST "http://localhost:5000/api/boards/save" \
-H "Content-Type: application/json" \
-d '{"state":"000\n111\n000"}'

🔹 Retrieve a Board State

curl -X GET "http://localhost:5000/api/boards/load/{id}"

Replace {id} with a valid board ID.

🐳 Running with Docker (Optional)

1️⃣ Build the Docker Image

docker build -t gameoflifeapi .

2️⃣ Run the Docker Container

docker run -p 5000:5000 gameoflifeapi

3️⃣ Verify API is Running

Visit http://localhost:5000/swagger to test endpoints.

🛠️ Explanation of the Solution & Thought Process

🔹 High-Level Design

REST API built using ASP.NET Core Web API.

Entity Framework Core (EF Core) for database operations.

SQLite as the database for persistence.

Docker support for easy deployment.

🔹 Key Features

Feature

Description

State Persistence

Saves board states to a database.

Next State Calculation

Computes the next state of the board.

Final State Detection

Detects if the board has stabilized.

Unit Tests

Uses xUnit to test core logic.

🧠 Thought Process

The board state is represented as a 2D array of 0 (dead) and 1 (alive).

EF Core handles board persistence efficiently.

Unit tests ensure the logic is correct.

Dockerization makes deployment easier.

⚖️ Assumptions & Trade-offs

✅ Assumptions

✔️ The board size is fixed at 3x3, but can be expanded.✔️ The initial state is provided as a string ("000\n111\n000").✔️ The API does not include authentication for simplicity.

⚠️ Trade-offs

⚠️ SQLite is used for simplicity, but a larger system may require PostgreSQL.⚠️ The Game of Life algorithm is computed synchronously, which could be optimized for large grids.