#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Full Epic Execution Orchestrator with BobCoin Budget Management.

This script orchestrates wave-based execution of all 173 epics across 9 phases,
with automatic BobCoin budget monitoring and refill prompts.

Usage:
    python scripts/orchestrate_full_epic_execution.py --start-phase 0 --wave-size 10

Features:
    - Wave-based parallel execution (default 10 epics per wave)
    - BobCoin balance monitoring before/after each wave
    - Cost-per-phase calculation and prediction
    - Automatic refill prompts when balance < 10 BobCoins
    - Progress tracking and resumption
    - Detailed cost reports
"""
import json
import sys
import io
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional

# Force UTF-8 encoding for Windows console
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

class BobCoinBudgetManager:
    """Manages BobCoin budget tracking and refill prompts."""
    
    def __init__(self, refill_threshold: float = 10.0, refill_amount: float = 160.0):
        self.refill_threshold = refill_threshold
        self.refill_amount = refill_amount
        self.cost_history: List[Dict] = []
        
    def check_balance(self) -> float:
        """
        Check current BobCoin balance.
        
        In production, this would query the actual Bob IDE balance.
        For now, returns a simulated balance.
        """
        # TODO: Integrate with actual Bob IDE balance API
        # For now, simulate balance tracking
        return 100.0  # Placeholder
    
    def record_cost(self, phase: int, wave: int, cost: float):
        """Record cost for a phase/wave execution."""
        self.cost_history.append({
            'phase': phase,
            'wave': wave,
            'cost': cost,
            'timestamp': datetime.now().isoformat()
        })
    
    def get_average_cost_per_phase(self, phase: int) -> Optional[float]:
        """Calculate average cost per epic for a specific phase."""
        phase_costs = [c['cost'] for c in self.cost_history if c['phase'] == phase]
        if not phase_costs:
            return None
        return sum(phase_costs) / len(phase_costs)
    
    def predict_wave_cost(self, phase: int, wave_size: int) -> Optional[float]:
        """Predict cost for next wave based on historical data."""
        avg_cost = self.get_average_cost_per_phase(phase)
        if avg_cost is None:
            return None
        return avg_cost * wave_size
    
    def needs_refill(self, balance: float, predicted_cost: Optional[float] = None) -> bool:
        """Check if refill is needed."""
        if balance < self.refill_threshold:
            return True
        if predicted_cost and balance < predicted_cost:
            return True
        return False
    
    def prompt_refill(self, current_balance: float, predicted_cost: Optional[float] = None):
        """Prompt user to refill BobCoins."""
        print("\n" + "=" * 80)
        print("BOBCOIN REFILL REQUIRED")
        print("=" * 80)
        print(f"Current Balance: {current_balance:.2f} BobCoins")
        if predicted_cost:
            print(f"Predicted Next Wave Cost: {predicted_cost:.2f} BobCoins")
        print(f"Recommended Refill: {self.refill_amount:.2f} BobCoins")
        print()
        print("Please refill your BobCoin balance and press Enter to continue...")
        print("Or type 'skip' to continue without refill (not recommended)")
        print("Or type 'quit' to stop execution")
        print("=" * 80)
        
        response = input("> ").strip().lower()
        
        if response == 'quit':
            print("\nExecution stopped by user.")
            sys.exit(0)
        elif response == 'skip':
            print("\nContinuing without refill (balance may be insufficient)...")
            return False
        else:
            print("\nAssuming refill complete. Continuing execution...")
            return True

class EpicWaveOrchestrator:
    """Orchestrates wave-based epic execution across all phases."""
    
    def __init__(self, wave_size: int = 10, start_phase: int = 0):
        self.wave_size = wave_size
        self.start_phase = start_phase
        self.budget_manager = BobCoinBudgetManager()
        self.roadmap = self._load_roadmap()
        self.pending_epics = self._get_pending_epics()
        
    def _load_roadmap(self) -> List[Dict]:
        """Load epic roadmap."""
        with open('epic_roadmap.json', 'r') as f:
            return json.load(f)
    
    def _get_pending_epics(self) -> List[Dict]:
        """Get list of pending epics."""
        return [e for e in self.roadmap if e.get('status') != 'complete']
    
    def _get_wave_epics(self, wave_num: int) -> List[Dict]:
        """Get epics for a specific wave."""
        start_idx = wave_num * self.wave_size
        end_idx = start_idx + self.wave_size
        return self.pending_epics[start_idx:end_idx]
    
    def _execute_phase_for_epic(self, epic: Dict, phase: int) -> Dict:
        """
        Execute a single phase for a single epic.
        
        In production, this would call the actual MCP tool.
        For now, returns a simulated result.
        """
        print(f"  [{epic['epic_number']}] Phase {phase}: {epic['method']} (CYC {epic['cyclomatic']})")
        
        # Simulate MCP tool call
        mcp_call = {
            'server': f'phase-{phase}-*',
            'tool': f'execute_phase_{phase}',
            'args': {
                'epic_id': epic['epic_number'],
                'method': epic['method'],
                'file': epic['file'],
                'cyc': epic['cyclomatic']
            }
        }
        
        # TODO: Replace with actual MCP tool call
        # result = use_mcp_tool(**mcp_call)
        
        # Simulate result
        return {
            'epic_id': epic['epic_number'],
            'phase': phase,
            'status': 'success',
            'cost': 0.5,  # Simulated cost
            'mcp_call': mcp_call
        }
    
    def execute_wave(self, phase: int, wave_num: int) -> Dict:
        """Execute a single wave (all epics in wave for one phase)."""
        wave_epics = self._get_wave_epics(wave_num)
        
        if not wave_epics:
            return {
                'phase': phase,
                'wave': wave_num,
                'epics_processed': 0,
                'total_cost': 0.0,
                'status': 'no_epics'
            }
        
        print(f"\n{'=' * 80}")
        print(f"WAVE {wave_num + 1} - PHASE {phase}")
        print(f"{'=' * 80}")
        print(f"Processing {len(wave_epics)} epics...")
        print()
        
        # Check balance before wave
        balance_before = self.budget_manager.check_balance()
        predicted_cost = self.budget_manager.predict_wave_cost(phase, len(wave_epics))
        
        print(f"BobCoin Balance: {balance_before:.2f}")
        if predicted_cost:
            print(f"Predicted Wave Cost: {predicted_cost:.2f}")
        print()
        
        # Check if refill needed
        if self.budget_manager.needs_refill(balance_before, predicted_cost):
            self.budget_manager.prompt_refill(balance_before, predicted_cost)
        
        # Execute phase for all epics in wave
        results = []
        for epic in wave_epics:
            result = self._execute_phase_for_epic(epic, phase)
            results.append(result)
        
        # Calculate wave cost
        total_cost = sum(r['cost'] for r in results)
        
        # Check balance after wave
        balance_after = self.budget_manager.check_balance()
        actual_cost = balance_before - balance_after
        
        print()
        print(f"Wave Complete:")
        print(f"  Epics Processed: {len(results)}")
        print(f"  Total Cost: {actual_cost:.2f} BobCoins")
        print(f"  Avg Cost/Epic: {actual_cost / len(results):.2f} BobCoins")
        print(f"  Balance After: {balance_after:.2f} BobCoins")
        
        # Record cost
        self.budget_manager.record_cost(phase, wave_num, actual_cost)
        
        return {
            'phase': phase,
            'wave': wave_num,
            'epics_processed': len(results),
            'total_cost': actual_cost,
            'avg_cost_per_epic': actual_cost / len(results),
            'balance_before': balance_before,
            'balance_after': balance_after,
            'status': 'success',
            'results': results
        }
    
    def execute_phase_all_waves(self, phase: int) -> List[Dict]:
        """Execute a single phase for all waves."""
        print(f"\n{'=' * 80}")
        print(f"PHASE {phase} EXECUTION - ALL WAVES")
        print(f"{'=' * 80}")
        print(f"Total Pending Epics: {len(self.pending_epics)}")
        print(f"Wave Size: {self.wave_size}")
        print(f"Total Waves: {(len(self.pending_epics) + self.wave_size - 1) // self.wave_size}")
        print()
        
        wave_results = []
        wave_num = 0
        
        while True:
            wave_epics = self._get_wave_epics(wave_num)
            if not wave_epics:
                break
            
            result = self.execute_wave(phase, wave_num)
            wave_results.append(result)
            wave_num += 1
        
        # Phase summary
        total_epics = sum(r['epics_processed'] for r in wave_results)
        total_cost = sum(r['total_cost'] for r in wave_results)
        avg_cost = total_cost / total_epics if total_epics > 0 else 0
        
        print(f"\n{'=' * 80}")
        print(f"PHASE {phase} COMPLETE")
        print(f"{'=' * 80}")
        print(f"Total Epics: {total_epics}")
        print(f"Total Waves: {len(wave_results)}")
        print(f"Total Cost: {total_cost:.2f} BobCoins")
        print(f"Avg Cost/Epic: {avg_cost:.2f} BobCoins")
        print()
        
        return wave_results
    
    def execute_all_phases(self):
        """Execute all phases for all epics."""
        print(f"\n{'=' * 80}")
        print("FULL EPIC EXECUTION - ALL PHASES")
        print(f"{'=' * 80}")
        print(f"Total Epics: {len(self.pending_epics)}")
        print(f"Starting Phase: {self.start_phase}")
        print(f"Wave Size: {self.wave_size}")
        print()
        
        all_results = {}
        
        for phase in range(self.start_phase, 7):  # Phases 0-6
            phase_results = self.execute_phase_all_waves(phase)
            all_results[f'phase_{phase}'] = phase_results
        
        # Final summary
        self._print_final_summary(all_results)
    
    def _print_final_summary(self, all_results: Dict):
        """Print final execution summary."""
        print(f"\n{'=' * 80}")
        print("FINAL EXECUTION SUMMARY")
        print(f"{'=' * 80}")
        print()
        
        total_cost = 0.0
        total_epics = 0
        
        for phase_key, phase_results in all_results.items():
            phase_num = int(phase_key.split('_')[1])
            phase_cost = sum(r['total_cost'] for r in phase_results)
            phase_epics = sum(r['epics_processed'] for r in phase_results)
            avg_cost = phase_cost / phase_epics if phase_epics > 0 else 0
            
            print(f"Phase {phase_num}:")
            print(f"  Epics: {phase_epics}")
            print(f"  Cost: {phase_cost:.2f} BobCoins")
            print(f"  Avg/Epic: {avg_cost:.2f} BobCoins")
            print()
            
            total_cost += phase_cost
            total_epics += phase_epics
        
        print(f"{'=' * 80}")
        print(f"GRAND TOTAL:")
        print(f"  Total Epics Processed: {total_epics}")
        print(f"  Total Cost: {total_cost:.2f} BobCoins")
        print(f"  Avg Cost/Epic: {total_cost / total_epics:.2f} BobCoins")
        print(f"{'=' * 80}")

def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(description='Orchestrate full epic execution with BobCoin management')
    parser.add_argument('--wave-size', type=int, default=10, help='Number of epics per wave (default: 10)')
    parser.add_argument('--start-phase', type=int, default=0, help='Starting phase (default: 0)')
    parser.add_argument('--single-phase', type=int, help='Execute only this phase')
    
    args = parser.parse_args()
    
    orchestrator = EpicWaveOrchestrator(
        wave_size=args.wave_size,
        start_phase=args.start_phase
    )
    
    try:
        if args.single_phase is not None:
            orchestrator.execute_phase_all_waves(args.single_phase)
        else:
            orchestrator.execute_all_phases()
    except KeyboardInterrupt:
        print("\n\nExecution interrupted by user")
        sys.exit(130)
    except Exception as e:
        print(f"\n\nFATAL ERROR: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

if __name__ == '__main__':
    main()

# Made with Bob
