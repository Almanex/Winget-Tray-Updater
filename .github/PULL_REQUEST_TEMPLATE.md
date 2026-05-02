name: Pull Request
description: Submit changes to the project
title: '[PR] '
labels: []
assignees: []
body:
  - type: markdown
    attributes:
      value: |
        ## 📝 Pull Request Description
        Please describe your changes.

  - type: textarea
    id: changes
    attributes:
      label: What changed?
      description: List the key changes made in this PR
      placeholder: |
        - Added new feature X
        - Fixed bug Y
        - Refactored component Z

  - type: textarea
    id: testing
    attributes:
      label: Testing
      description: How has this been tested?
      placeholder: |
        - [ ] Tested locally on Windows 11
        - [ ] Verified tray icon displays correctly
        - [ ] Tested notification functionality

  - type: textarea
    id: screenshots
    attributes:
      label: Screenshots (if applicable)
      description: Include screenshots of UI changes

  - type: checkboxes
    id: checklist
    attributes:
      label: 
      options:
        - label: My code follows the project's coding style
          required: true
        - label: I have read the Contributing guidelines
          required: true
        - label: My changes generate no new warnings
          required: true
        - label: I have added/updated documentation (if applicable)
          required: true