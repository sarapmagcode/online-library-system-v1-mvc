# Online Library System

The **Online Library System** is a web application designed to manage books, categories, and user interactions such as borrowing and returning books. It provides functionalities for both users and administrators, allowing users to browse and borrow books, while administrators can manage books and categories.

## Features

### User Features
- **User Authentication**: Users can register, log in, and log out.
- **Browse Books**: Users can view a list of available books with details such as title, author, description, and availability.
- **Borrow Books**: Users can borrow books, with a due date set for return.
- **Return Books**: Users can return borrowed books.
- **Extend Borrowing Period**: Users can extend the due date for borrowed books.
- **Profile Management**: Users can view their profile, borrowed books, and update their password.

### Admin Features
- **Manage Books**: Admins can add, edit, and delete books. They can also upload book images.
- **Manage Categories**: Admins can add, edit, and delete book categories.
- **View Borrowing Statistics**: Admins can view statistics related to book borrowing.

## Technologies Used

- **ASP.NET Core**: The backend framework used for building the web application.
- **Entity Framework Core**: Used for database operations and management.
- **SQL Server**: The database used to store application data.
- **Bootstrap**: Used for front-end styling and responsive design.
- **Razor Views**: Used for creating dynamic web pages.
- **Session Management**: Used for managing user sessions and authentication.

## Getting Started

### Prerequisites

- **.NET SDK**: Ensure you have the .NET SDK installed on your machine.
- **SQL Server**: Ensure you have SQL Server installed and running.
- **Visual Studio** (optional): Recommended for easier development and debugging.

### Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/your-username/online-library-system.git
   cd online-library-system
   ```
2. **Set Up the Database**:
   - Update the connection string in appsettings.json to point to your SQL Server instance.
   - Run the following commands to apply migrations and seed the database:
     ```bash
     dotnet ef database update
     ```
3. **Run the Application**:
   ```bash
   dotnet run
   ```
   - The application will be available at http://localhost:5000.

### Usage

- **User Login**: Use the login page to access the system. If you don't have an account, you can register as a new user.
- **Admin Access**: To access admin features, log in with an account that has the admin role.
- **Borrowing Books**: Users can browse books and borrow them. Borrowed books will appear in the user's profile.
- **Returning Books**: Users can return borrowed books from their profile.
- **Managing Books and Categories**: Admins can manage books and categories from the respective sections.

### Project Structure

- **Controllers**: Contains the controllers for handling user requests (e.g., AccountController, BooksController, CategoriesController).
- **Models**: Contains the entity models and view models used in the application.
- **Views**: Contains the Razor views for rendering the UI.
- **Data**: Contains the database context (i.e., entities, relationships).
- **Migrations**: Contains the database migrations.
- **wwwroot**: Contains static files such as images, CSS, and JavaScript.

### Contributing

Contributions are welcome! If you'd like to contribute, please follow these steps:
1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes.
4. Push your branch and submit a pull request.

### Acknowledgments

- Thanks to the ASP.NET Core team for providing a robust framework for building web applications.
- Thanks to the Bootstrap team for providing a responsive and easy-to-use front-end framework.

### Contact

For any questions or feedback, you can contact me at markjasongalang.work@gmail.com
