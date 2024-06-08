﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObject.Entities;
using DataTransfer;
using DataTransfer.Shipper;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class OrderDAO
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        private static OrderDAO instance;

        public OrderDAO()
        {
            _context = new AppDbContext();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AutoMapperProfile.AutoMapperProfile())).CreateMapper();
        }

        public static OrderDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OrderDAO();
                }
                return instance;
            }
        }

        public async Task<List<OrderDTO>> GetAllOrders()
        {
            List<OrderDTO> orderDTO = null;
            try
            {
                var order = await _context.Orders.ToListAsync();
                orderDTO = _mapper.Map<List<OrderDTO>>(order);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return orderDTO;
        }

        public async Task AddOrderAsync(Order order)
        {
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log detailed error message and inner exception details
                Console.WriteLine($"Error in AddOrderAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw new Exception("An error occurred while adding the order.", ex);
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<OrderHistoryDTO>> GetAllOrderHistory(int userId)
        {
            List<OrderHistoryDTO> list = null;
            try
            {
                var l = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(o => o.Milk)
                    .Include(o => o.PaymentType)
                    .Include(o => o.Shipper)
                    .Include(o => o.OrderStatus)
                    .Where(o => o.BuyerId == userId)
                    .ToListAsync();
                list = _mapper.Map<List<OrderHistoryDTO>>(l);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return list;
        }

        public async Task<List<OrderDetailHistoryDTO>> GetDetailHistory(int orderId)
        {
            List<OrderDetailHistoryDTO> list = null;
            try
            {
                var l = await _context.OrderDetails
                    .Include(o => o.Milk)
                    .Where(o => o.OrderId == orderId)
                    .ToListAsync();
                list = _mapper.Map<List<OrderDetailHistoryDTO>>(l);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return list;
        }

        public async Task<List<ShipperOrderDTO>> GetShipperAssignedOrder(int shipperId)
        {
            try
            {
                var shipper = await _context.Shippers.Where(x => x.AppUserId == shipperId).FirstOrDefaultAsync();
                if (shipper == null)
                {
                    throw new Exception("Do not find Shipper");
                }
                var orders = await _context.Orders.Where(x => x.ShipperId == shipper.Id)
                    .ProjectTo<ShipperOrderDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                return orders;
            }
            catch (Exception ex)
            {
                throw new Exception("Error");
            }
        }
        public async Task<ShipperOrderDetailDTO> GetShipperOrderDetail(int shipperId, int orderId)
        {
            try
            {
                var shipper = await _context.Shippers.Where(x => x.AppUserId == shipperId).FirstOrDefaultAsync();
                if (shipper == null)
                {
                    throw new Exception("Do not find Shipper");
                }
                var order = await _context.Orders.Where(x => x.ShipperId == shipper.Id && x.Id == orderId)
                    .ProjectTo<ShipperOrderDetailDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                return order;
            }
            catch (Exception ex)
            {
                throw new Exception("Error");
            }
        }

        public async Task<bool> ConfirmShipped(int shipperId, int orderId)
        {
            try
            {
                var shipper = await _context.Shippers.Where(x => x.AppUserId == shipperId).FirstOrDefaultAsync();
                if (shipper == null)
                {
                    throw new Exception("Do not find Shipper");
                }
                var order = await _context.Orders.Where(x => x.ShipperId == shipper.Id && x.Id == orderId)
                    .FirstOrDefaultAsync();
                order.OrderStatusId = 3;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ConfirmCancelled(int shipperId, int orderId)
        {
            try
            {
                var shipper = await _context.Shippers.Where(x => x.AppUserId == shipperId).FirstOrDefaultAsync();
                if (shipper == null)
                {
                    throw new Exception("Do not find Shipper");
                }
                var order = await _context.Orders.Where(x => x.ShipperId == shipper.Id && x.Id == orderId)
                    .FirstOrDefaultAsync();
                order.OrderStatusId = 4;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task AutoAssignOrdersToShippers()
        {
            var currentTime = DateTime.Now;
            var timeSlots = await _context.TimeSlots.ToListAsync();

            foreach (var timeSlot in timeSlots)
            {
                var slotStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, timeSlot.StartTime.Hours, timeSlot.StartTime.Minutes, 0);
                if (currentTime >= slotStartTime.AddHours(-1) && currentTime <= slotStartTime.AddHours(timeSlot.EndTime.Hours - timeSlot.StartTime.Hours))
                {
                    var orders = await _context.Orders
                        .Where(o => o.TimeSlotId == timeSlot.Id && o.OrderStatusId == 2 && o.ShipperId == null)
                        .Include(o => o.Address)
                        .ToListAsync();

                    foreach (var order in orders)
                    {
                        var districtId = order.Address.DistrictId;
                        var availableShippers = await _context.Shippers
                            .Where(s => s.DistrictId == districtId && s.Status == "Available")
                            .ToListAsync();

                        if (availableShippers.Any())
                        {
                            var shipper = availableShippers.First();
                            order.ShipperId = shipper.Id;
                            shipper.Status = "Shipping";

                            _context.Update(order);
                            _context.Update(shipper);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }
    }

}
