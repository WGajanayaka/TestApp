
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dushmantha Baranige	
-- Create date: 2017-03-30
-- Description:	Get supplier payment MOE paid details
-- =============================================
IF (OBJECT_ID('GetSupplierPaymentMOEPaidDetail') IS NOT NULL)
  DROP PROCEDURE GetSupplierPaymentMOEPaidDetail
GO

CREATE PROCEDURE dbo.GetSupplierPaymentMOEPaidDetail
	@Year AS INT,
	@Month AS NVARCHAR(20),
	@ForTxtWrite AS BIT = 0
	
AS
BEGIN
	
	SET NOCOUNT ON;
	IF @ForTxtWrite = 1
	BEGIN
	SELECT H.PaymentID AS PaymentNo , H.PaymentDate AS PaymentDate , H.Year , H.Month ,H.BankName , D.AccountNo ,D.BankCode , D.BranchCode, H.ChequeNumber ,H.ChequeDate ,H.Amount ,S.SupplierName
					FROM SupplierPayment_MOE_Header AS H
					INNER JOIN SupplierPayment_MOE_Details AS D
					ON H.PaymentID = D.PaymentID
					INNER JOIN m_SupplierInformation  AS S
					ON S.ID = D.SupplierID
					WHERE H.Year = @Year AND H.Month = @Month 
	END
	BEGIN
	SELECT H.PaymentID AS PaymentNo , H.PaymentDate AS PaymentDate , H.Year , H.Month ,H.BankName , H.ChequeNumber ,H.ChequeDate ,H.Amount  FROM SupplierPayment_MOE_Header AS H					
					WHERE H.Year = @Year AND H.Month = @Month 
	END
	
					
END
GO
