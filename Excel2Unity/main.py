from Excel2Unity import Excel2Unity
import UIPageGen


def main():
    Excel2Unity().Process()
    uipageGen = UIPageGen.UIPageGen()
    uipageGen.gen_ui_page()


if __name__ == '__main__':
    main()
