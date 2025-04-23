using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class CommentDAO
    {
        private static CommentDAO instance = null;
        private readonly DataContext _context;

        private CommentDAO()
        {
            _context = new DataContext();
        }

        public static CommentDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CommentDAO();
                }
                return instance;
            }
        }

        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments
                .Include(c => c.VideoHistory)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetAllActiveCommentsAsync()
        {
            return await _context.Comments
                .Include(c => c.VideoHistory)
                .Where(c => c.Quantity > 0)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.VideoHistory)
                .FirstOrDefaultAsync(c => c.CommentID == commentId);
        }

        public async Task<bool> AddCommentAsync(Comment comment)
        {
            bool vhExists = await _context.VideoHistories
                .AnyAsync(v => v.VideoHistoryID == comment.VideoHistoryID);
            if (!vhExists) return false;

            bool accountExists = await _context.Accounts
                .AnyAsync(a => a.AccountID == comment.AccountID);
            if (!accountExists) return false;

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            var existingComment = await GetCommentByIdAsync(comment.CommentID);
            if (existingComment == null)
                return false;

            _context.Entry(existingComment).CurrentValues.SetValues(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await GetCommentByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.Quantity = 0;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Comment>> GetCommentsWithAccountByVideoHistoryIdAsync(int videoHistoryId)
        {
            return await _context.Comments
                .Where(c => c.VideoHistoryID == videoHistoryId && c.Quantity > 0)
                .Include(c => c.VideoHistory)
                .ToListAsync();
        }

        public async Task<int> GetTotalCommentsForVideoAsync(int videoHistoryId)
        {
            return await _context.Comments
                .Where(c => c.VideoHistoryID == videoHistoryId && c.Quantity > 0)
                .SumAsync(c => c.Quantity);
        }
    }
}
