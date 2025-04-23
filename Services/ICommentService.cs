using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetAllCommentsAsync();
        Task<List<Comment>> GetAllActiveCommentsAsync();
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<bool> AddCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<List<Comment>> GetCommentsWithAccountByVideoHistoryIdAsync(int videoHistoryId);
        Task<int> GetTotalCommentsForVideoAsync(int videoHistoryId);
    }
}
