using Salamtak.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IChatService
    {
        Task<string> SendMessageAsync(Guid patientId, string userMessage);
        Task<List<ChatMessage>> GetHistoryAsync(Guid patientId);
    }
}