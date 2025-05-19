using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Dtos.MotherboardDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.MotherboardMappers
{
    public class MotherboardMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Motherboard;
        public MotherboardMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var motherboard = (Motherboard)entity;
            var offers = productOffers.Where(p => p.ComponentId == motherboard.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);
            var cpuPowerConnectorDtos = motherboard.CpuPowerConnectors
                .Select(c => new CpuPowerConnectorDto
                {
                    Pins = c.Pins,
                    Quantity = c.Quantity
                })
                .ToList();
            var pcleSlotDtos = motherboard.PcleSlots
                .Select(c => new PcleSlotDto
                {
                    Version = c.Version,
                    Lane = c.Lane,
                    Quantity = c.Quantity
                })
                .ToList();
            var m2SlotDtos = motherboard.M2Slots
                .Select(c => new M2SlotDto
                {
                    Version = c.Version,
                    Lane = c.Lane,
                    Quantity = c.Quantity
                })
                .ToList();
            var rearPortDtos = motherboard.RearPorts
                .Select(c => new RearPortDto
                {
                    Type = c.Type,
                    Quantity = c.Quantity
                })
                .ToList();
            var innerPortDtos = motherboard.InnerPorts
                .Select(c => new InnerPortDto
                {
                    Type = c.Type,
                    Value = c.Value
                })
                .ToList();

            return new MotherboardDto
            {
                Id = motherboard.Id,
                Name = motherboard.Name,
                PhotoUrl = motherboard.PhotoUrl,
                Description = motherboard.Description,
                Brand = motherboard.Brand,
                Socket = motherboard.Socket,
                Chipset = motherboard.Chipset,
                DimmSlots = motherboard.DimmSlots,
                DimmType = motherboard.DimmType,
                DimmFrequency = motherboard.DimmFrequency,
                DimmCapacity = motherboard.DimmCapacity,
                Sata3Count = motherboard.Sata3Count,
                PowerMotherboard = motherboard.PowerMotherboard,
                FanQuantity = motherboard.FanQuantity,
                PcleX1Quantity = motherboard.PcleX1Quantity,
                Ethernet = motherboard.Ethernet,
                Audio = motherboard.Audio,
                Wifi = motherboard.Wifi,
                Bluetooth = motherboard.Bluetooth,
                VideoPorts = motherboard.VideoPorts,
                FormFactor = motherboard.FormFactor,
                SizeDimentions = motherboard.SizeDimentions,
                Wattage = motherboard.Wattage,
                FactoryLink = motherboard.FactoryLink,
                AveragePrice = motherboard.AveragePrice,
                OffersCount = motherboard.OffersCount,
                CpuPowerConnectors = cpuPowerConnectorDtos,
                PcleSlots = pcleSlotDtos,
                M2Slots = m2SlotDtos,
                RearPorts = rearPortDtos,
                InnerPorts = innerPortDtos,
                ProductOffers = offerDtos
            };
        }

    }
}
