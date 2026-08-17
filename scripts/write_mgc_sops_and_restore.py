import sys, math, os
sys.stdout.reconfigure(encoding='utf-8')

NT_ATM  = r'C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\templates\AtmStrategy'
NT_STOP = r'C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\templates\StopStrategy'

# ── MGC SOP standalone StopStrategy XMLs ──────────────────────────────────
# MES: 4 tk/pt.  MGC: 10 tk/pt.  Scale factor = 10/4 = 2.5
# MES SOP trail triggers: @T1*1.5tk, @T1*2tk, @T1*2.5tk  (SOP3: 12,16,20 for T1=8)
# MGC equivalents keep same POINT distances → multiply MES ticks by 2.5
#
# MGC SOP3  — used by SL4(T1=20), SL5(T1=25), SL6(T1=30)
#   Representative T1 = 20tk (SL4)
#   BE trigger = 20tk (2.0pt)
#   Step1: profit trigger=30tk(3.0pt) → stop=10tk(1.0pt)
#   Step2: profit trigger=40tk(4.0pt) → stop=25tk(2.5pt)
#   Step3: profit trigger=50tk(5.0pt) → stop=40tk(4.0pt)
#
# MGC SOP35 — T1=35tk (SL7)
#   BE=35, Step1: 52→17, Step2: 70→43, Step3: 87→70
#
# MGC SOP4  — T1=40tk (SL8)
#   BE=40, Step1: 60→20, Step2: 80→50, Step3: 100→80
#
# MGC SOP45 — T1=45tk (SL9)
#   BE=45, Step1: 67→22, Step2: 90→56, Step3: 112→90
#
# MGC SOP5  — T1=50tk (SL10)
#   BE=50, Step1: 75→25, Step2: 100→62, Step3: 125→100

SOP_DEFS = [
    # name,       BE,  pt1, sl1,  pt2, sl2,  pt3, sl3,  label
    ('MGC SOP3',  20,  30,  10,   40,  25,   50,  40,  'MGC SOP3'),
    ('MGC SOP35', 35,  52,  17,   70,  43,   87,  70,  'MGC SOP35'),
    ('MGC SOP4',  40,  60,  20,   80,  50,  100,  80,  'MGC SOP4'),
    ('MGC SOP45', 45,  67,  22,   90,  56,  112,  90,  'MGC SOP45'),
    ('MGC SOP5',  50,  75,  25,  100,  62,  125, 100,  'MGC SOP5'),
]

SOP_XML = '''<?xml version="1.0" encoding="utf-8"?>
<NinjaTrader>
  <StopStrategy xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <AutoBreakEvenPlus>2</AutoBreakEvenPlus>
    <AutoBreakEvenProfitTrigger>{be}</AutoBreakEvenProfitTrigger>
    <AutoTrailSteps>
      <AutoTrailStep><Frequency>2</Frequency><ProfitTrigger>{pt1}</ProfitTrigger><StopLoss>{sl1}</StopLoss></AutoTrailStep>
      <AutoTrailStep><Frequency>2</Frequency><ProfitTrigger>{pt2}</ProfitTrigger><StopLoss>{sl2}</StopLoss></AutoTrailStep>
      <AutoTrailStep><Frequency>1</Frequency><ProfitTrigger>{pt3}</ProfitTrigger><StopLoss>{sl3}</StopLoss></AutoTrailStep>
    </AutoTrailSteps>
    <IsSimStopEnabled>false</IsSimStopEnabled>
    <VolumeTrigger>0</VolumeTrigger>
    <Template>{label}</Template>
  </StopStrategy>
</NinjaTrader>'''

print('=== Writing MGC SOP StopStrategy files ===')
for name, be, pt1, sl1, pt2, sl2, pt3, sl3, label in SOP_DEFS:
    xml = SOP_XML.format(be=be, pt1=pt1, sl1=sl1, pt2=pt2, sl2=sl2,
                         pt3=pt3, sl3=sl3, label=label)
    fpath = os.path.join(NT_STOP, f'{name}.xml')
    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(xml)
    print(f'  ✅ {name}.xml  BE={be}tk  trails: @{pt1}→{sl1}, @{pt2}→{sl2}, @{pt3}→{sl3}')

# ── Restore damaged MGC $400 SL5 and SL6 ATM XMLs ─────────────────────────
# SOP_MAP and trail inline — same scale formula as above, keyed by T1 ticks
SOP_MAP = {4:'MGC SOP3',5:'MGC SOP3',6:'MGC SOP3',
           7:'MGC SOP35',8:'MGC SOP4',9:'MGC SOP45',10:'MGC SOP5'}

QTY_400 = {4:10, 5:8, 6:6, 7:5, 8:5, 9:4, 10:4}

ATM_XML = '''<?xml version="1.0" encoding="utf-8"?>
<NinjaTrader>
  <AtmStrategy xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <IsVisible>true</IsVisible>
    <calculate2>OnBarClose</calculate2>
    <AreLinesConfigurable>true</AreLinesConfigurable>
    <ArePlotsConfigurable>true</ArePlotsConfigurable>
    <BarsToLoad>0</BarsToLoad>
    <Calculate>OnBarClose</Calculate>
    <Displacement>0</Displacement>
    <DisplayInDataBox>true</DisplayInDataBox>
    <From>2099-12-01T00:00:00</From>
    <IsAutoScale>true</IsAutoScale>
    <Lines />
    <MaximumBarsLookBack>TwoHundredFiftySix</MaximumBarsLookBack>
    <Name>AtmStrategy</Name>
    <Panel>0</Panel>
    <Plots />
    <ScaleJustification>Right</ScaleJustification>
    <ShowTransparentPlotsInDataBox>false</ShowTransparentPlotsInDataBox>
    <To>1800-01-01T00:00:00</To>
    <IsDataSeriesRequired>false</IsDataSeriesRequired>
    <IsOverlay>false</IsOverlay>
    <SelectedValueSeries>0</SelectedValueSeries>
    <Gtd>1800-01-01T00:00:00</Gtd>
    <Template>{name}</Template>
    <TimeInForce>Gtc</TimeInForce>
    <BarsRequiredToTrade>0</BarsRequiredToTrade>
    <Category>Atm</Category>
    <ConnectionLossHandling>KeepRunning</ConnectionLossHandling>
    <DaysToLoad>1</DaysToLoad>
    <DefaultQuantity>1</DefaultQuantity>
    <DisconnectDelaySeconds>0</DisconnectDelaySeconds>
    <EntriesPerDirection>1</EntriesPerDirection>
    <EntryHandling>AllEntries</EntryHandling>
    <ExitOnSessionCloseSeconds>0</ExitOnSessionCloseSeconds>
    <IncludeCommission>false</IncludeCommission>
    <IsAggregated>false</IsAggregated>
    <IsExitOnSessionCloseStrategy>false</IsExitOnSessionCloseStrategy>
    <IsFillLimitOnTouch>false</IsFillLimitOnTouch>
    <IsOptimizeDataSeries>false</IsOptimizeDataSeries>
    <IsStableSession>false</IsStableSession>
    <IsTickReplay>false</IsTickReplay>
    <IsTradingHoursBreakLineVisible>false</IsTradingHoursBreakLineVisible>
    <IsWaitUntilFlat>false</IsWaitUntilFlat>
    <NumberRestartAttempts>0</NumberRestartAttempts>
    <OptimizationPeriod>10</OptimizationPeriod>
    <OrderFillResolution>High</OrderFillResolution>
    <OrderFillResolutionType>Tick</OrderFillResolutionType>
    <OrderFillResolutionValue>1</OrderFillResolutionValue>
    <RestartsWithinMinutes>0</RestartsWithinMinutes>
    <SetOrderQuantity>Strategy</SetOrderQuantity>
    <Slippage>0</Slippage>
    <StartBehavior>AdoptAccountPosition</StartBehavior>
    <StopTargetHandling>PerEntryExecution</StopTargetHandling>
    <SupportsOptimizationGraph>false</SupportsOptimizationGraph>
    <TestPeriod>28</TestPeriod>
    <TradingHoursSerializable />
    <Brackets>
{brackets}    </Brackets>
    <EntryQuantity>{total}</EntryQuantity>
    <CalculationMode>Ticks</CalculationMode>
    <ChaseLimit>1</ChaseLimit>
    <InitialTickSize>0</InitialTickSize>
    <IsChase>false</IsChase>
    <IsChaseIfTouched>true</IsChaseIfTouched>
    <IsTargetChase>false</IsTargetChase>
    <ReverseAtStop>false</ReverseAtStop>
    <ReverseAtTarget>false</ReverseAtTarget>
    <UseMitForProfit>false</UseMitForProfit>
    <UseStopLimitForStopLossOrders>false</UseStopLimitForStopLossOrders>
    <AtmSelector>508659aee0554887b3aaf1944107b792</AtmSelector>
    <OnBehalfOf />
    <ReverseAtStopStrategyId>-1</ReverseAtStopStrategyId>
    <ReverseAtTargetStrategyId>-1</ReverseAtTargetStrategyId>
    <ShadowStrategyStrategyId>-1</ShadowStrategyStrategyId>
    <ShadowTemplate />
  </AtmStrategy>
</NinjaTrader>'''

B1 = '''      <Bracket>
        <Quantity>{q}</Quantity>
        <StopLoss>{sl}</StopLoss>
        <Target>{t}</Target>
      </Bracket>
'''

BSOP = '''      <Bracket>
        <Quantity>{q}</Quantity>
        <StopLoss>{sl}</StopLoss>
        <StopStrategy>
          <AutoBreakEvenPlus>2</AutoBreakEvenPlus>
          <AutoBreakEvenProfitTrigger>{be}</AutoBreakEvenProfitTrigger>
          <AutoTrailSteps>
            <AutoTrailStep><Frequency>2</Frequency><ProfitTrigger>{pt1}</ProfitTrigger><StopLoss>{sl1}</StopLoss></AutoTrailStep>
            <AutoTrailStep><Frequency>2</Frequency><ProfitTrigger>{pt2}</ProfitTrigger><StopLoss>{sl2}</StopLoss></AutoTrailStep>
            <AutoTrailStep><Frequency>1</Frequency><ProfitTrigger>{pt3}</ProfitTrigger><StopLoss>{sl3}</StopLoss></AutoTrailStep>
          </AutoTrailSteps>
          <IsSimStopEnabled>false</IsSimStopEnabled>
          <VolumeTrigger>0</VolumeTrigger>
          <Template>{sop}</Template>
        </StopStrategy>
        <Target>{t}</Target>
      </Bracket>
'''

def make_atm_xml(sl_pts, total, risk_label):
    sl_tk  = sl_pts * 10
    t1_tk  = sl_tk // 2
    t2_tk  = int(sl_tk * 0.75)
    t3_tk  = sl_tk
    q1 = math.ceil(total / 2)
    q2 = math.ceil((total - q1) / 2)
    q3 = total - q1 - q2
    sop   = SOP_MAP[sl_pts]
    pt1 = int(t1_tk * 1.5);  sl1 = int(t1_tk * 0.5)
    pt2 = int(t1_tk * 2.0);  sl2 = int(t1_tk * 1.25)
    pt3 = int(t1_tk * 2.5);  sl3 = int(t1_tk * 2.0)
    name = f'MGC {risk_label} SL{sl_pts}'

    b1 = B1.format(q=q1, sl=sl_tk, t=t1_tk)
    b2 = BSOP.format(q=q2, sl=sl_tk, be=t1_tk,
                     pt1=pt1, sl1=sl1, pt2=pt2, sl2=sl2, pt3=pt3, sl3=sl3,
                     sop=sop, t=t2_tk)
    brackets = b1 + b2
    if q3 > 0:
        b3 = BSOP.format(q=q3, sl=sl_tk, be=t1_tk,
                         pt1=pt1, sl1=sl1, pt2=pt2, sl2=sl2, pt3=pt3, sl3=sl3,
                         sop=sop, t=t3_tk)
        brackets += b3
    return ATM_XML.format(name=name, brackets=brackets, total=total), name

print('\n=== Restoring damaged MGC ATM XMLs (SL5, SL6) ===')
for sl_pts in [5, 6]:
    total = QTY_400[sl_pts]
    xml, name = make_atm_xml(sl_pts, total, '$400')
    fpath = os.path.join(NT_ATM, f'{name}.xml')
    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(xml)
    sl_tk = sl_pts * 10; t1 = sl_tk//2; t2 = int(sl_tk*0.75)
    q1 = math.ceil(total/2); q2 = math.ceil((total-q1)/2); q3 = total-q1-q2
    print(f'  ✅ {name}  Q={q1}+{q2}+{q3}  T1={t1}  T2={t2}  T3={sl_tk}  SOP={SOP_MAP[sl_pts]}')

# Also update SL4 inline template label from "SOP3" → "MGC SOP3"
# (SL4 ATM XML is intact but references "SOP3" label — update all intact $400 MGC ATMs for consistency)
print('\n=== Updating MGC SOP label in all MGC $400 ATMs (SOP3 → MGC SOP3 etc.) ===')
for sl_pts in [4, 7, 8, 9, 10]:  # SL5/6 already written fresh above
    total = QTY_400[sl_pts]
    xml, name = make_atm_xml(sl_pts, total, '$400')
    fpath = os.path.join(NT_ATM, f'{name}.xml')
    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(xml)
    print(f'  ✅ {name}  label updated to {SOP_MAP[sl_pts]}')

# Same for all MGC $200 ATMs
print('\n=== Updating all MGC $200 ATMs ===')
QTY_200 = {4:5, 5:4, 6:3, 7:3, 8:2, 9:2, 10:2}
for sl_pts in [4,5,6,7,8,9,10]:
    total = QTY_200[sl_pts]
    xml, name = make_atm_xml(sl_pts, total, '$200')
    fpath = os.path.join(NT_ATM, f'{name}.xml')
    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(xml)
    print(f'  ✅ {name}  label updated to {SOP_MAP[sl_pts]}')

print('\nDone.')
