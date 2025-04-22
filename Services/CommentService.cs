using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepo _commentRepo;

        public CommentService(ICommentRepo commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            return await _commentRepo.GetAllCommentsAsync();
        }
        public async Task<List<Comment>> GetAllActiveCommentsAsync()
        {
            return await _commentRepo.GetAllActiveCommentsAsync();
        }
        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _commentRepo.GetCommentByIdAsync(commentId);
        }

        public async Task<bool> AddCommentAsync(Comment comment)
        {
            return await _commentRepo.AddCommentAsync(comment);
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            return await _commentRepo.UpdateCommentAsync(comment);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _commentRepo.DeleteCommentAsync(commentId);
        }

        public async Task<int> GetTotalCommentsForVideoAsync(int videoHistoryId)
        {
            return await _commentRepo.GetTotalCommentsForVideoAsync(videoHistoryId);
        }

        public async Task<List<Comment>> GetCommentsWithAccountByVideoHistoryIdAsync(int videoHistoryId)
        {
            return await _commentRepo.GetCommentsWithAccountByVideoHistoryIdAsync(videoHistoryId);
        }
    }

}
