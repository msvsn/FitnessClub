using System;
using FitnessClub.DAL.Entities;
using FitnessClub.Core.Abstractions;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingStrategy
    {
        Booking CreateBooking(int? userId, int classScheduleId, DateTime classDate, UserDto? user);
    }
}