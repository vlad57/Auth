namespace API_Custom.Services.Interfaces
{
    public interface IRazerViewRenderer
    {
        public Task<string> RenderViewToStringAsync<TModel>(string path, TModel model);
    }
}
