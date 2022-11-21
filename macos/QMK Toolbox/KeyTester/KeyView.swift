import AppKit

@IBDesignable
class KeyView: NSView {
    @IBOutlet var contentView: NSView!
    @IBOutlet var boxView: NSBox!
    @IBOutlet var legendView: NSTextField!

    @IBInspectable
    var pressed: Bool = false {
        didSet {
            needsDisplay = true
        }
    }

    @IBInspectable
    var tested: Bool = false {
        didSet {
            needsDisplay = true
        }
    }

    @IBInspectable
    var legend: String = "" {
        didSet {
            legendView.stringValue = legend
            needsDisplay = true
        }
    }

    override init(frame frameRect: NSRect) {
        super.init(frame: frameRect)
        customInit()
    }

    required init?(coder: NSCoder) {
        super.init(coder: coder)
        customInit()
    }

    override func draw(_ dirtyRect: NSRect) {
        super.draw(dirtyRect)
        boxView.fillColor = pressed ? NSColor.systemYellow : tested ? NSColor.systemGreen : NSColor.clear
        legendView.textColor = (pressed || tested) ? NSColor.black : NSColor.secondaryLabelColor
    }

    func customInit() {
        let bundle = Bundle(for: type(of: self))
        let nib = NSNib(nibNamed: "KeyView", bundle: bundle)!
        nib.instantiate(withOwner: self, topLevelObjects: nil)
        addSubview(contentView)

        var newConstraints: [NSLayoutConstraint] = []
        contentView.constraints.forEach { oldConstraint in
            let firstItem: NSView = oldConstraint.firstItem!.isEqual(to: contentView) ? self : oldConstraint.firstItem as! NSView
            let secondItem: NSView = oldConstraint.secondItem!.isEqual(to: contentView) ? self : oldConstraint.secondItem as! NSView

            newConstraints.append(NSLayoutConstraint(item: firstItem, attribute: oldConstraint.firstAttribute, relatedBy: oldConstraint.relation, toItem: secondItem, attribute: oldConstraint.secondAttribute, multiplier: oldConstraint.multiplier, constant: oldConstraint.constant))

            contentView.subviews.forEach { newView in
                addSubview(newView)
            }

            addConstraints(newConstraints)
        }
    }
}
