using FluentAssertions;
using SocialHub.Domain.Comments;
using Xunit;
 
namespace SocialHub.Domain.Tests.Comments;
 
public class CommentReportTests
{
    [Fact]
    public void Create_Should_SetAllFields()
    {
        var commentId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
 
        var report = CommentReport.Create(commentId, reporterId, CommentReportReason.Spam, "looks like spam");
 
        report.CommentId.Should().Be(commentId);
        report.ReporterId.Should().Be(reporterId);
        report.Reason.Should().Be(CommentReportReason.Spam);
        report.Details.Should().Be("looks like spam");
    }
 
    [Fact]
    public void Create_Should_AllowNullDetails()
    {
        var report = CommentReport.Create(Guid.NewGuid(), Guid.NewGuid(), CommentReportReason.Other);
 
        report.Details.Should().BeNull();
    }
}