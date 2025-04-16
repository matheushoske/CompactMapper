using System;
using System.Collections.Generic;
using CompactMapper;

namespace CompactMapperExample
{
    // Source entities
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
    }

    // Target DTOs
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } // Custom mapping
        public AddressDto Address { get; set; }
        public List<OrderDto> Orders { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create sample data
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "Example City",
                    PostalCode = "12345"
                },
                Orders = new List<Order>
                {
                    new Order { Id = 101, OrderDate = DateTime.Now, Total = 125.99m },
                    new Order { Id = 102, OrderDate = DateTime.Now.AddDays(-5), Total = 249.95m }
                }
            };

            // Register custom mapping for Customer to CustomerDto
            CompactMapperExtension.AddCustomMapping<Customer, CustomerDto>((src, dest) =>
            {
                dest.FullName = $"{src.FirstName} {src.LastName}";
            });

            // Map customer to DTO
            var customerDto = customer.MapTo<CustomerDto>();

            // Display result
            Console.WriteLine($"Customer ID: {customerDto.Id}");
            Console.WriteLine($"Full Name: {customerDto.FullName}");
            Console.WriteLine($"Address: {customerDto.Address.Street}, {customerDto.Address.City}, {customerDto.Address.PostalCode}");
            Console.WriteLine($"Orders: {customerDto.Orders.Count}");
            
            foreach (var order in customerDto.Orders)
            {
                Console.WriteLine($"  - Order #{order.Id}: {order.OrderDate:d} - ${order.Total}");
            }
        }
    }
} 