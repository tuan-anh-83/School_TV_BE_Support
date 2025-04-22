using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class CommentRepo : ICommentRepo
    {
        public async Task<bool> AddCommentAsync(Comment comment)
        {
            return await CommentDAO.Instance.AddCommentAsync(comment);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await CommentDAO.Instance.DeleteCommentAsync(commentId);
        }

        public async Task<List<Comment>> GetAllActiveCommentsAsync()
        {
            return await CommentDAO.Instance.GetAllActiveCommentsAsync();

        }

        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            return await CommentDAO.Instance.GetAllCommentsAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await CommentDAO.Instance.GetCommentByIdAsync(commentId);
        }

        public async Task<List<Comment>> GetCommentsWithAccountByVideoHistoryIdAsync(int videoHistoryId)
        {
            return await CommentDAO.Instance.GetCommentsWithAccountByVideoHistoryIdAsync(videoHistoryId);
        }

        public async Task<int> GetTotalCommentsForVideoAsync(int videoHistoryId)
        {
            return await CommentDAO.Instance.GetTotalCommentsForVideoAsync(videoHistoryId);
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            return await CommentDAO.Instance.UpdateCommentAsync(comment);
        }
    }
}
