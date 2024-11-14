using AutoMapper;
using OfficeOpenXml;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Aspiration
{
    public class AspirationService : IAspirationService
    {
        private readonly IAspirationRepository _aspirationRepository;
        private readonly IMapper _mapper;

        public AspirationService(IAspirationRepository aspirationRepository, IMapper mapper)
        {
            _aspirationRepository = aspirationRepository;
            _mapper = mapper;
        }

        public async Task<Pagination<AspirationModel>> GetAllAsync(AspirationQueryModel queryModel)
        {
            return await _aspirationRepository.GetAllAsync(queryModel);
        }

        public async Task<AspirationModel> GetByIdAsync(Guid id)
        {
            var aspiration = await _aspirationRepository.GetByIdAsync(id);
            return _mapper.Map<AspirationModel>(aspiration);
        }

        public async Task AddAsync(AspirationModel aspirationModel)
        {
            var newAspiration = _mapper.Map<Data.Aspiration>(aspirationModel);
            await _aspirationRepository.AddAsync(newAspiration);
        }

        public async Task UpdateAsync(AspirationModel aspirationModel)
        {
            var updateAspiration = _mapper.Map<Data.Aspiration>(aspirationModel);
            await _aspirationRepository.UpdateAsync(updateAspiration);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _aspirationRepository.DeleteAsync(id);
        }

        public async Task<bool> ImportAspirationsAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var aspirations = new List<Data.Aspiration>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 3; row <= rowCount; row++)
                    {
                        string studentName = worksheet.Cells[row, 3].Text.Trim();

                        if (string.IsNullOrEmpty(studentName))
                        {
                            // Bỏ qua dòng này nếu không đáp ứng điều kiện
                            continue;
                        }

                        string desireAccept = worksheet.Cells[row, 16].Text.Trim();
                        if (string.IsNullOrEmpty(desireAccept))
                        {
                            continue;
                        }


                        var aspiration = new Data.Aspiration
                        {
                            Id = Guid.NewGuid(),
                            StudentId = worksheet.Cells[row, 2].Text.Trim(),
                            StudentName = studentName,
                            Topic = worksheet.Cells[row, 7].Text.Trim(),
                            ClassName = worksheet.Cells[row, 8].Text.Trim(),
                            GroupName = worksheet.Cells[row, 6].Text.Trim(),
                            Status = worksheet.Cells[row, 15].Text.Trim(),
                            DesireAccept = worksheet.Cells[row, 16].Text.Trim(),
                            Aspiration1 = worksheet.Cells[row, 17].Text.Trim(),
                            Aspiration2 = worksheet.Cells[row, 18].Text.Trim(),
                            Aspiration3 = worksheet.Cells[row, 19].Text.Trim(),
                            StatusCode = desireAccept == "Chờ xác nhận" ? 0 : 1
                        };

                        aspirations.Add(aspiration);

                    }
                }
            }

            await _aspirationRepository.AddRangeAsync(aspirations);
            return true;
        }
    }
}
