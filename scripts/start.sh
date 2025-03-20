Town:                   Moscow|Saint_Petersburg|Kazan|Yoshar-Ola, Vladivostok
LoanPurpose:            NewBuilding, SecondaryHousing, BuildHouse, Another
ProgramType:            Base, Family, FarEasternArctic, IT, LandPlots, None
Price:                  1725000, 50862500, 100000000, none
InitialPayment:         6720573, 10227786, 13735000
IsMaternityCapital:     Yes, No
MaternityCapitalAmount: 1000, 456081, 912162, none
Term:                   1, 15, 30
IsCurrentSalaryCardVTB: Yes, No
IsTransferInVTB:        Yes, No
IsDigitalServices:      Yes, No
IsInsurance:            Yes, No
IsMyRate:               Yes, No
IsSalaryClient:         Yes, No
RateTime:               none, 1, 2, 3, 4, 5, 7, all
RateSize:               none, 0.5, 1, 1.5, 2, 2.5

if [LoanPurpose] = "NewBuilding" then [ProgramType] = "Base" or [ProgramType] = "Family" or [ProgramType] = "FarEasternArctic" or [ProgramType] = "IT";
if [LoanPurpose] = "SecondaryHousing" then [ProgramType] = "Base" or [ProgramType] = "FarEasternArctic" or [ProgramType] =  "LandPlots";
if [LoanPurpose] = "BuildHouse" then [ProgramType] = "Base" or [ProgramType] = "Family";
if [LoanPurpose] = "Another" then [ProgramType] = "None" and [IsMaternityCapital] = "No";

if [ProgramType] = "Base" or [ProgramType] = "LandPlots" then [IsSalaryClient] = "No";
if [ProgramType] = "Base" and [LoanPurpose] = "BuildHouse" then [IsTransferInVTB] = "No" and [IsDigitalServices] = "No" and [IsMyRate] = "No" and [IsSalaryClient] = "No";
if [ProgramType] = "Family" then [IsCurrentSalaryCardVTB] = "No" and [IsTransferInVTB] = "No" and [IsMyRate] = "No";
if [ProgramType] = "Family" and [LoanPurpose] = "NewBuilding" then [IsDigitalServices] = "No";

if [ProgramType] = "Family" and [LoanPurpose] = "BuildHouse" then [IsSalaryClient] = "No";
if [ProgramType] = "FarEasternArctic" or [ProgramType] = "IT" then [IsCurrentSalaryCardVTB] = "No" and [IsTransferInVTB] = "No" and [IsDigitalServices] = "No" and [IsMyRate] = "No" and [IsSalaryClient] = "No";
if [ProgramType] = "FarEasternArctic" and [Town] <> "Vladivostok" then [Price] = "none";
if [ProgramType] = "LandPlots" or [LoanPurpose] = "Another" then [IsMaternityCapital] = "No";

if [IsMaternityCapital] = "No" then [MaternityCapitalAmount] = "none";
if [IsCurrentSalaryCardVTB] = "Yes" then [IsTransferInVTB] = "No" and [IsDigitalServices] = "No";
if [IsTransferInVTB] = "Yes" then [IsCurrentSalaryCardVTB] = "No" and [IsDigitalServices] = "No" and [IsMyRate] = "No";
if [IsDigitalServices] = "Yes" then [IsCurrentSalaryCardVTB] = "No";
if [IsMyRate] = "Yes" then [IsTransferInVTB] = "No" and [RateTime] <> "none" and [RateSize] <> "none";
if [IsMyRate] = "No" then [RateTime] = "none" and [RateSize] = "none";
if [Term] < 15 then [RateTime] = "all";
