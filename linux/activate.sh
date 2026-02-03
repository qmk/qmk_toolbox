#!/bin/bash
# Activation script for QMK Toolbox development environment

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Activate virtual environment
source "$SCRIPT_DIR/venv/bin/activate"

# Display environment info
echo "QMK Toolbox Development Environment"
echo "===================================="
echo "Python: $(which python)"
echo "Python version: $(python --version)"
echo "Installed packages:"
pip list | grep -E "(PySide6|pyudev|hidapi|qmk-toolbox)" | column -t
echo ""
echo "Run the application with: qmk-toolbox"
echo "Or run directly: python -m qmk_toolbox.main"
echo ""
