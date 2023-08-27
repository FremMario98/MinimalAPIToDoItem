using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

var toDoItems = app.MapGroup("/todoitems");

toDoItems.MapGet("/", async (TodoDb db) =>
{
    return await db.Todos.ToListAsync();
});

toDoItems.MapGet("/complete", async (TodoDb db) =>
{
    return await db.Todos.Where(todo => todo.IsComplete).ToListAsync();
});

toDoItems.MapGet("/{id}", async(int id, TodoDb db) =>
{
    Todo? todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == id);
    return todo is not null ? Results.Ok(todo) : Results.NotFound();
});

toDoItems.MapPost("/", async (Todo todo, TodoDb db) =>
{
    if (string.IsNullOrEmpty(todo.Name))
    {
        return Results.BadRequest();
    }

    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

toDoItems.MapPut("/{id}", async (int id, Todo todo, TodoDb db) =>
{
    Todo? oldToDo = await db.Todos.FirstOrDefaultAsync(t => t.Id == id);

    if(oldToDo is null)
    {
        return Results.NotFound();
    }

    oldToDo.IsComplete = todo.IsComplete;
    oldToDo.Name = todo.Name;

    await db.SaveChangesAsync();

    return Results.NoContent(); // Success
});

toDoItems.MapDelete("/{id}", async(int id, TodoDb db) =>
{
    Todo? todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == id);

    if(todo is null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
