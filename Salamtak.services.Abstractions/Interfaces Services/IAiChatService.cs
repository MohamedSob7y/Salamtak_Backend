using Salamtak.Shared.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAiChatService
    {
        Task<ChatResponseDto> GetMedicalSpecialtyAsync(ChatRequestDto request);

    }
}
