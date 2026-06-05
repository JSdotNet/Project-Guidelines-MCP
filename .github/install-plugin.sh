#!/usr/bin/env bash

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Defaults
PLUGIN_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SKILLS_PATH="${SKILLS_PATH:-$HOME/.copilot/skills}"
EXTENSIONS_PATH="${EXTENSIONS_PATH:-$HOME/.copilot/extensions}"

PLUGIN_SKILLS_DIR="$PLUGIN_ROOT/.github/skills"
PLUGIN_EXTENSIONS_DIR="$PLUGIN_ROOT/.github/extensions"

echo -e "${CYAN}📦 JSdotNet Project Guidelines Plugin Installer${NC}"
echo ""
echo -e "${GREEN}Installation Configuration:${NC}"
echo "  Plugin Root:       $PLUGIN_ROOT"
echo "  Skills source:     $PLUGIN_SKILLS_DIR"
echo "  Skills target:     $SKILLS_PATH"
echo "  Extensions source: $PLUGIN_EXTENSIONS_DIR"
echo "  Extensions target: $EXTENSIONS_PATH"
echo ""

# Verify source directories exist
if [ ! -d "$PLUGIN_SKILLS_DIR" ]; then
    echo -e "${RED}❌ Error: Skills directory not found at $PLUGIN_SKILLS_DIR${NC}"
    exit 1
fi

if [ ! -d "$PLUGIN_EXTENSIONS_DIR" ]; then
    echo -e "${RED}❌ Error: Extensions directory not found at $PLUGIN_EXTENSIONS_DIR${NC}"
    exit 1
fi

# Create target directories
mkdir -p "$SKILLS_PATH"
mkdir -p "$EXTENSIONS_PATH"

# Copy skills
echo -e "${CYAN}Installing Skills...${NC}"
for skill_file in "$PLUGIN_SKILLS_DIR"/*.md; do
    if [ -f "$skill_file" ]; then
        cp "$skill_file" "$SKILLS_PATH/"
        echo -e "  ${GREEN}✓ $(basename "$skill_file")${NC}"
    fi
done

# Copy extensions
echo ""
echo -e "${CYAN}Installing Extensions...${NC}"
for extension_dir in "$PLUGIN_EXTENSIONS_DIR"/*/; do
    if [ -d "$extension_dir" ]; then
        ext_name=$(basename "$extension_dir")
        target_ext_path="$EXTENSIONS_PATH/$ext_name"
        
        # Remove existing if present
        if [ -d "$target_ext_path" ]; then
            rm -rf "$target_ext_path"
        fi
        
        # Copy new
        cp -r "$extension_dir" "$target_ext_path"
        echo -e "  ${GREEN}✓ $ext_name${NC}"
    fi
done

echo ""
echo -e "${GREEN}✅ Installation Complete!${NC}"
echo ""
echo -e "${CYAN}Next Steps:${NC}"
echo "  1. Restart your Copilot CLI session"
echo "  2. Run: extensions_reload"
echo "  3. Invoke a skill:"
echo "     • Skill: guidelines-mcp"
echo "     • Skill: gap-analysis"
echo "     • Skill: feedback-loop"
echo ""
echo -e "${CYAN}Need help? See .github/PLUGIN_README.md${NC}"
