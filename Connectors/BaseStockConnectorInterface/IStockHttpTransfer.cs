using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseStockConnectorInterface.Models;
using BaseStockConnectorInterface.Models.History;

namespace BaseStockConnectorInterface
{
    public interface IStockHttpTransfer
    {
        public Task<ExchangeResponse<List<DepositItemModel>>> GetDepositHistory(string asset, DateTime? dateFromUtc = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeResponse<List<WithdrawalItemModel>>> GetWithdrawHistory(string asset, DateTime? dateFromUtc = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeResponse<List<TransferItemModel>>> GetTransferHistory(string asset, int limit, DateTime? dateFromUtc = null)
        {
            throw new NotImplementedException();
        }
    }
}
