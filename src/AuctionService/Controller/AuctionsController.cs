using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controller
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
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions= await _context.Auctions
                                .Include(a => a.Item)
                                .OrderBy(a=>a.Item.Make)
                                .ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                                .Include(a => a.Item)
                                .FirstOrDefaultAsync(a=>a.Id == id);                                

            if (auction == null) return NotFound();
            
            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);    
            auction.Seller="test";
            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if  (!result) BadRequest("Could not save to database");

            return CreatedAtAction(nameof(GetAuctionById), new {auction.Id},_mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auction)
        {
            var auctionEntity =await _context.Auctions.Include(a=>a.Item).FirstOrDefaultAsync(a=>a.Id == id);
            if (auctionEntity == null) return NotFound();
            
            auctionEntity.Item.Make = auction.Make??auctionEntity.Item.Make;
            auctionEntity.Item.Model = auction.Model??auctionEntity.Item.Model;
            auctionEntity.Item.Year = auction.Year ?? auctionEntity.Item.Year;
            auctionEntity.Item.Mileage = auction.Mileage??auctionEntity.Item.Mileage;
            auctionEntity.Item.Color = auction.Color??auctionEntity.Item.Color;

            var result = await _context.SaveChangesAsync() > 0;

            if (result)                
                return Ok();

            return BadRequest("Problem saving changes");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();

            _context.Auctions.Remove(auction);
            var result = await _context.SaveChangesAsync()>0;

            if (result) return Ok();

            return BadRequest("Could not remove auction");
        }
    }
}
