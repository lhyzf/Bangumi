using System.Threading.Tasks;

namespace Bangumi.Views
{
    public interface IPageStatus
    {
        Task Refresh();
        bool IsLoading { get; }
    }
}
