using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAuctions()
        {
            var auctions = await _context.Auctions
                .Include(i => i.Item)
                .OrderBy(i => i.Item.Make)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<AuctionDto>>(auctions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (auction == null)
                return NotFound();
            return Ok(_mapper.Map<AuctionDto>(auction));
        }

        [HttpPost]

        public async Task<ActionResult<Auction>> CreatAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);
            auction.Seller = "test";
            await _context.AddAsync(auction);

            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
                return BadRequest("cant add this to the database");
            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
                return NotFound();
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
                return Ok();

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
                return NotFound();
            _context.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest("Can't delete this item");
        }
    }
}
