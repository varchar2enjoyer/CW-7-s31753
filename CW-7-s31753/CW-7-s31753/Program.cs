using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using CW_7_s31753.Models;
using CW_7_s31753.Repositories;
using CW_7_s31753.Validators;
using CW_7_s31753.Database;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Travel Agency API", 
        Version = "v1",
        Description = "API for managing travel agency operations" 
    });
});

// Register dependencies
builder.Services.AddSingleton<DbConnection>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddValidatorsFromAssemblyContaining<ClientValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global error handling
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(new
        {
            Message = "An unexpected error occurred. Please try again later.",
            DateTime = DateTime.UtcNow
        });
    });
});

// Endpoints
app.MapGet("/api/trips", async (ITripRepository repo) =>
{
    var trips = await repo.GetAllTripsAsync();
    return Results.Ok(trips);
})
.WithName("GetAllTrips")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get all available trips",
    Description = "Returns all trips with their details and associated countries"
});

app.MapGet("/api/clients/{id:int}/trips", async (int id, ITripRepository repo) =>
{
    if (!await repo.ClientExistsAsync(id))
        return Results.NotFound($"Client with ID {id} not found");

    var trips = await repo.GetClientTripsAsync(id);
    
    return trips.Any() 
        ? Results.Ok(trips) 
        : Results.NotFound($"No trips found for client with ID {id}");
})
.WithName("GetClientTrips")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get trips for a specific client",
    Description = "Returns all trips associated with the given client ID"
});

app.MapPost("/api/clients", async (Client client, ITripRepository repo, IValidator<Client> validator) =>
{
    var validationResult = await validator.ValidateAsync(client);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    try
    {
        var clientId = await repo.AddClientAsync(client);
        return Results.Created($"/api/clients/{clientId}", new { Id = clientId });
    }
    catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
    {
        return Results.Conflict("Client with this PESEL or Email already exists");
    }
})
.WithName("CreateClient")
.WithOpenApi(operation => new(operation)
{
    Summary = "Create a new client",
    Description = "Creates a new client record with validation"
});

app.MapPut("/api/clients/{clientId:int}/trips/{tripId:int}", 
    async (int clientId, int tripId, ITripRepository repo, DateTime? paymentDate) =>
    {
        if (!await repo.ClientExistsAsync(clientId))
            return Results.NotFound($"Client with ID {clientId} not found");

        if (!await repo.TripExistsAsync(tripId))
            return Results.NotFound($"Trip with ID {tripId} not found");

        var currentParticipants = await repo.GetTripParticipantsCountAsync(tripId);
        var trip = (await repo.GetAllTripsAsync()).FirstOrDefault(t => t.Id == tripId);
        
        if (trip != null && currentParticipants >= trip.MaxPeople)
            return Results.BadRequest("This trip has reached maximum capacity");

        var success = await repo.AssignClientToTripAsync(clientId, tripId, paymentDate);
        
        return success 
            ? Results.Ok("Client successfully registered for the trip") 
            : Results.BadRequest("Failed to register client for the trip");
    })
    .WithName("AssignClientToTrip")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Register a client for a trip",
        Description = "Assigns a client to a specific trip with optional payment date"
    });

app.MapDelete("/api/clients/{clientId:int}/trips/{tripId:int}", 
    async (int clientId, int tripId, ITripRepository repo) =>
    {
        var success = await repo.RemoveClientFromTripAsync(clientId, tripId);
        
        return success 
            ? Results.NoContent() 
            : Results.NotFound("Registration not found or already removed");
    })
    .WithName("RemoveClientFromTrip")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Remove client from a trip",
        Description = "Cancels a client's registration for a specific trip"
    });

app.Run();