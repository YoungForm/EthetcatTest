using Microsoft.AspNetCore.Mvc;
using EtherCATTestApi.EtherCATTestEngine.SII;

namespace EtherCATTestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SIIController : ControllerBase
    {
        private readonly SIIEditor _siiEditor;

        public SIIController()
        {
            _siiEditor = new SIIEditor();
        }

        // GET: api/SII/ReadByte/0
        [HttpGet("ReadByte/{address}")]
        public ActionResult<byte> ReadByte(uint address)
        {
            try
            {
                var value = _siiEditor.ReadByte(address);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SII/WriteByte
        [HttpPost("WriteByte")]
        public ActionResult WriteByte([FromBody] SIIWriteRequest request)
        {
            try
            {
                _siiEditor.WriteByte(request.Address, (byte)request.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SII/ReadWord/0
        [HttpGet("ReadWord/{address}")]
        public ActionResult<ushort> ReadWord(uint address)
        {
            try
            {
                var value = _siiEditor.ReadWord(address);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SII/WriteWord
        [HttpPost("WriteWord")]
        public ActionResult WriteWord([FromBody] SIIWriteRequest request)
        {
            try
            {
                _siiEditor.WriteWord(request.Address, (ushort)request.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SII/ReadDWord/0
        [HttpGet("ReadDWord/{address}")]
        public ActionResult<uint> ReadDWord(uint address)
        {
            try
            {
                var value = _siiEditor.ReadDWord(address);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SII/WriteDWord
        [HttpPost("WriteDWord")]
        public ActionResult WriteDWord([FromBody] SIIWriteRequest request)
        {
            try
            {
                _siiEditor.WriteDWord(request.Address, (uint)request.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SII/ReadBlock/0/16
        [HttpGet("ReadBlock/{address}/{length}")]
        public ActionResult<byte[]> ReadBlock(uint address, uint length)
        {
            try
            {
                var data = _siiEditor.ReadBlock(address, length);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SII/WriteBlock
        [HttpPost("WriteBlock")]
        public ActionResult WriteBlock([FromBody] SIIWriteBlockRequest request)
        {
            try
            {
                _siiEditor.WriteBlock(request.Address, request.Data);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SII/GetAll
        [HttpGet("GetAll")]
        public ActionResult<byte[]> GetAll()
        {
            try
            {
                var data = _siiEditor.GetSIIData();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SII/SetAll
        [HttpPost("SetAll")]
        public ActionResult SetAll([FromBody] byte[] data)
        {
            try
            {
                _siiEditor.SetSIIData(data);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SII/Clear
        [HttpPost("Clear")]
        public ActionResult Clear()
        {
            try
            {
                _siiEditor.Clear();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SII/Validate
        [HttpGet("Validate")]
        public ActionResult<SIIValidationResult> Validate()
        {
            try
            {
                var result = _siiEditor.Validate();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    // Request models
    public class SIIWriteRequest
    {
        public uint Address { get; set; }
        public ulong Value { get; set; }
    }

    public class SIIWriteBlockRequest
    {
        public uint Address { get; set; }
        public byte[] Data { get; set; }
    }
}