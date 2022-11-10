using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RNG = UnityEngine.Random;
using KModkit;
using System;
using CharSlotsTools;
using KeepCoding;


public class CharacterSlotsScript : ModuleScript
{

	public KMBombInfo bomb;
	public KMAudio sfx;
	public KMBombModule module;
	public Image[] slots;
	public Sprite[] sprites;
	public GameObject plzHelp;

	public KMSelectable[] keepButtons;
	public KMSelectable crank;
	public KMSelectable[] stageButtons;

	public Material[] unlitMats;
	public MeshRenderer[] keepStatusMat;
	public MeshRenderer[] stageStatusMat;

	bool[] keepStates = new bool[3] { false, false, false };
	Character[,] slotStates = new Character[3, 3];
	int stageNumber = 0; //Machine-indexed

	private bool userInputPossible = false;

	// Use this for initialization
	void Start()
	{
		module.OnActivate += delegate () { StartCoroutine(CycleAndSelect()); };
		keepButtons.Assign(onInteract: KeepToggle);
		stageButtons.Assign(onInteract: RememberStage);
		crank.Assign(onInteract: TestStage);
		foreach (MeshRenderer ball in keepStatusMat) ball.material = unlitMats[0];
		foreach (MeshRenderer ball in stageStatusMat) ball.material = unlitMats[0];
	}

	private void TestStage()
	{
		ButtonEffect(crank, 1, "thrill");
		
		if (!userInputPossible||IsSolved) return;
		bool isGood = true;
		if(keepStates.All(ks=>ks) && slotStates[stageNumber,0].CharacterName== slotStates[stageNumber, 1].CharacterName && slotStates[stageNumber, 0].CharacterName== slotStates[stageNumber, 2].CharacterName) 
			Log("All 3 characters are the same!!!");
        else
			for (int c = 0; c <= 2; c++)
			{
				isGood = (ValidityCheck(c) == keepStates[c]);
				if (!isGood) break;
			}
		if (isGood)
		{
			Log("All keep states are correct. Stage {0} done!", stageNumber+1);
			stageStatusMat[stageNumber].material = unlitMats[2];
			stageNumber++;
			StartCoroutine(CycleAndSelect());
		}
		else
		{
			Log("Your answer for this character is incorrect. Strike!");
			Strike();
		}
	}

	private bool ValidityCheck(int c, bool isAutosolving = false)
    {
		Log("Checking validy for {0}...", slotStates[stageNumber, c].CharacterName);
		if (slotStates[stageNumber, c].CharacterName == CharacterName.MiiFighter)
		{
			if ((keepStates[c]||isAutosolving) && stageNumber != 0 && slotStates[stageNumber, c] == slotStates[stageNumber - 1, c])
			{
				Log("You already kept that Mii Fighter last stage ! That's illegal !");
				return false;
			}
			Log("Mii Fighter's special rules are respected.");
			return isAutosolving?true:keepStates[c];
		}
		try
		{
			int score = CalculateScore(slotStates[stageNumber, c], c);
			Log("{0} has a score of {1}, so you should {2}keep it.", slotStates[stageNumber, c].CharacterName, score, score <= 0 ? "not " : "");
			return (score > 0);
		}
		catch (Exception e)
		{
			plzHelp.SetActive(true);
			Log("An exception has occured. Please tell Konoko that \"{0}\" isn't a valid command for {1}. This character will be considered as valid.".Form(e.Message, slotStates[stageNumber, c].CharacterName), LogType.Exception);
			return true;
		}
	}

	private void RememberStage(int stage)
	{
		ButtonEffect(keepButtons[stage],1,"thrill");
		if (!userInputPossible || stage >= stageNumber || IsSolved) return;
		StartCoroutine(Memento(stage));
	}

	IEnumerator Memento(int stage)
    {
		userInputPossible = false;
		slots.ForEach(c => c.color = new Color32(0, 118, 255,255));
		for(int i = 0; i < 3; i++)
        {
			slots[i].sprite = sprites[(int)slotStates[stage, i].CharacterName];
        }
		for (int i = 0; i < 100; i++)
        {
			Color tmpColor = slots[0].color;
			tmpColor.a = Mathf.Abs(Mathf.Cos(i/3));
			slots.ForEach(s => s.color = tmpColor);
			yield return new WaitForSeconds(.005f);
        } 
		slots.ForEach(c => c.color = new Color32(255, 255, 255, 255));
		for (int i = 0; i < 3; i++)
		{
			slots[i].sprite = sprites[(int)slotStates[stageNumber, i].CharacterName];
		}
		userInputPossible = true;
    }

	private void KeepToggle(int i)
	{
		ButtonEffect(keepButtons[i],.5f,"thrill");
		if (!userInputPossible) return;
		keepStates[i] = !keepStates[i];
		keepStatusMat[i].material = keepStates[i] ? unlitMats[1] : unlitMats[0];
	}

	IEnumerator CycleAndSelect()
	{
		userInputPossible = false;
		if (keepStates.All(x => x))
		{
			Log("Premature ending : you kept every slot... And you're right!");
		}
		if (stageNumber != 3) stageStatusMat[stageNumber].material = unlitMats[3];
		for (int slotWheel = 0; slotWheel < 3; slotWheel++)
		{
            if (!keepStates[slotWheel]||stageNumber==3|| keepStates.All(x => x))
            {
				int j = -1;
				if (stageNumber == 0) slots[slotWheel].enabled = true;
				for (int rollTime = 0; rollTime < 50; rollTime++)
				{
					j = RNG.Range(0, Enum.GetValues(typeof(CharacterName)).Length);
					slots[slotWheel].sprite = sprites[j];
					yield return new WaitForSeconds(.01f);
				}
				if (stageNumber == 3|| keepStates.All(x => x)) slots[slotWheel].enabled = false;
				else slotStates[stageNumber, slotWheel] = new Character((CharacterName)j);
				PlaySound("sub_event_0" + slotWheel);
			}
			else slotStates[stageNumber, slotWheel] = slotStates[stageNumber - 1, slotWheel];

		}
		if (stageNumber == 3 || keepStates.All(x => x))
		{
			yield return new WaitForSeconds(.75f);
			Solve();
			PlaySound("sub_event_03");
			stageStatusMat.All(s => s.material = unlitMats[2]);
			Log("Module solved !");
		}

		if (!IsSolved)
		{
			Log("The appearing characters on stage {0} are : {1}, {2} & {3}", stageNumber + 1, slotStates[stageNumber, 0].CharacterName, slotStates[stageNumber, 1].CharacterName, slotStates[stageNumber, 2].CharacterName);
		}
		userInputPossible = true;
	}

	private sbyte CalculateScore(Character character, int position)
    {
		return (sbyte)(PrefrenceCheck(character) + SerialCheck(character) + ModulePreferenceCheck(character) + ConditionsCheck(character, position));
    }

	private sbyte SerialCheck(Character character)
    {
		sbyte res = 0;
		foreach(char c in character.SerialPreference)
        {
			if (bomb.GetSerialNumber().Contains(c))
            {
				Log("The character {0} is present in the serial number. +1 point.", c);
				res += 1;
			}
            //else
				//Log("The character {0} is absent in the serial number.", c);

        }
		return res;
    }

	private sbyte PrefrenceCheck(Character character)
    {
		string[] positions = new string[] { "left", "middle", "right" };
		sbyte res = 0;
		List<Character> seenChars = new List<Character>();
		for(sbyte stage = 0; stage <= stageNumber; stage++)
        {
			for(sbyte slot = 0; slot < 3; slot++)
            {
                if ((slotStates[stage, slot]!=character) && !seenChars.Contains(slotStates[stage, slot])) //Same instance = ignore
                {
					if (character.LikedCharacters.Contains(slotStates[stage, slot].CharacterName))
                    {
						Log("{0} likes {1} who is/was on the {2} slot of stage {3}. +1 point.",character.CharacterName, slotStates[stage, slot].CharacterName,positions[slot],stage+1);
						res += 1;
					}
						
					else if (character.DislikedCharacters.Contains(slotStates[stage, slot].CharacterName))
                    {
						Log("{0} dislikes {1} who is/was on the {2} slot of stage {3}. -1 point.", character.CharacterName, slotStates[stage, slot].CharacterName, positions[slot], stage + 1);
						res -= 1;
					}
					//else
					//Log("{0} feels neutral towards {1} who is/was on the {2} slot of stage {3}.", character.CharacterName, slotStates[stage, slot].CharacterName, positions[slot], stage + 1);
					seenChars.Add(slotStates[stage, slot]);
				}

            }
        }
		return res;
    }

	private sbyte ModulePreferenceCheck(Character character)
    {
		sbyte res = 0;

		foreach (string likedMod in character.LikedModules) res += ModuleCheck(likedMod,'+');
		foreach (string dislikedMod in character.DislikedModules) res -= ModuleCheck(dislikedMod,'-');

		return res;
    }

	private sbyte ModuleCheck(string modId,char posOrNeg)
	{
		sbyte ans = 0;
		List<string> mods = bomb.GetModuleIDs(); mods.Remove("characterSlots");
		if (mods.Contains(modId))
		{
			Log("A module with the id {0} is present on the bomb. {1}1 point.", modId, posOrNeg);
			ans++;
			if (bomb.GetSolvedModuleIDs().Contains(modId))
			{
				Log("A module with the id {0} is solved on the bomb. Another {1}1 point.", modId, posOrNeg);
				ans++;
			}
		}
        //else
			//Log("There are no modules with the id {0} present.", modId);
		return ans;
	}

	private sbyte ConditionsCheck(Character character, int position)
    {
		bool trueStatement = ConditionCheck(character.LikedEdgework, position, character);
		bool falseStatement = ConditionCheck(character.DislikedEdgework, position, character);
		Log("{0}'s for statement is {1}.{2}", character.CharacterName, trueStatement, trueStatement ? " +2 points." : "");
		Log("{0}'s against statement is {1}.{2}", character.CharacterName, falseStatement, falseStatement ? " -2 points." : "");
		return (sbyte)((trueStatement ? 2 : 0) - (falseStatement ? 2 : 0));
	}

	private bool ConditionCheck(string condition, int position, Character c)
    {
		bool isTrue=false;
		string[] condis = condition.Split(' ');
        switch (condis[0])
        {
			case "sn":
				switch (condis[1])
				{
					case "present":
						isTrue = bomb.GetSerialNumber().Contains(condis[2]);
						break;
					case "sum":
						isTrue = Comparer.SpecialNumberCompare(bomb.GetSerialNumberNumbers().Sum(), condis[2]);
						break;
					case "last":
						isTrue = Comparer.SpecialNumberCompare(int.Parse(bomb.GetSerialNumber()[5].ToString()), condis[2]);
						break;
					case "dr":
						isTrue = Comparer.SpecialNumberCompare(1 + (bomb.GetSerialNumberNumbers().Sum() - 1) % 9, condis[2]);
						break;
					case "motif":
						isTrue = Comparer.IsLetterOrNumber(bomb.GetSerialNumber()[0], condis[2][0]) && Comparer.IsLetterOrNumber(bomb.GetSerialNumber()[1], condis[2][1]);
						break;
					case "count":
						if (condis[2].Equals("X")) isTrue = Comparer.Compare(bomb.GetSerialNumberLetters().Count(), int.Parse(condis[4]), condis[3]);
						else if (condis[2].Equals("#")) isTrue = Comparer.Compare(bomb.GetSerialNumberNumbers().Count(), int.Parse(condis[4]), condis[3]);
						else throw new ArgumentException(condis[2] + " is not X nor #");
						break;
                }
				break;
			case "needy":
				isTrue = (bomb.GetModuleIDs().Count() - bomb.GetSolvableModuleIDs().Count() != 0);
				break;
			case "stage":
				isTrue = stageNumber == int.Parse(condis[1]) - 1;
				break;
			case "slot":
				string[] positions = new string[] { "L", "M", "R" };
				isTrue = positions.IndexOf(condis[1]) == position;
				break;
			case "ind":
                switch (condis[1])
                {
					case "identify":
						if (condis[2] == "lit") isTrue = bomb.GetOnIndicators().ToList().Contains(condis[3]);
						else if (condis[2] == "unlit") isTrue = bomb.GetOffIndicators().ToList().Contains(condis[3]);
						else if (condis[2] == "any") isTrue = bomb.GetIndicators().ToList().Contains(condis[3]);
						else throw new ArgumentException(condis[2] + " is not a valid argument.");
						break;
					case "compare":
						if (condis[2] == "lit") isTrue = bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count();
						else if (condis[2] == "unlit") isTrue = bomb.GetOffIndicators().Count() > bomb.GetOnIndicators().Count();
						else if (condis[2] == "exact") isTrue = bomb.GetOnIndicators().Count() == bomb.GetOffIndicators().Count();
						else throw new ArgumentException(condis[2] + " is not a valid argument.");
						break;
					default:
						if (condis[1] == "lit") isTrue = Comparer.Compare(bomb.GetOnIndicators().Count(),int.Parse(condis[3]),condis[2]);
						else if (condis[1] == "unlit") isTrue = Comparer.Compare(bomb.GetOffIndicators().Count(), int.Parse(condis[3]), condis[2]);
						else if (condis[1] == "any") isTrue = Comparer.Compare(bomb.GetIndicators().Count(), int.Parse(condis[3]), condis[2]);
						else throw new ArgumentException(condis[1] + " is not a valid argument.");
						break;
                }
				break;
			case "str":
				if (condis[1].Equals("1left")) isTrue = Game.Mission.GeneratorSetting.NumStrikes- bomb.GetStrikes()==1;
				else isTrue = Comparer.Compare(bomb.GetStrikes(), int.Parse(condis[2]), condis[1]);
				break;
			case "modules":
                if (condis[1].Equals("compare"))
                {
					if (condis[2].Equals("solved")) condis[2] = "more";
					else if (condis[2].Equals("unsolved")) condis[2] = "less";
					isTrue = Comparer.Compare(bomb.GetSolvedModuleIDs().Count(), bomb.GetModuleIDs().Count(), condis[2]);
                }
                else
                {
					float numberModules = condis[1] == "total" ? bomb.GetModuleIDs().Count() : bomb.GetSolvedModuleIDs().Count();
					if (condis[2].Equals("divisible")) isTrue = numberModules % int.Parse(condis[3]) == 0 && numberModules!=0;
                    else
                    {
						if (condis[2].Equals("percent")) numberModules = (float)(numberModules / bomb.GetModuleIDs().Count()) * 100;
						isTrue = Comparer.Compare(numberModules, int.Parse(condis[4]), condis[3]);
                    }
                }
				break;
			case "kept":
				if (stageNumber == 0) break;
				isTrue = slotStates[stageNumber - 1, position] == c;
				break;
			case "holders":
				isTrue = Comparer.Compare(bomb.GetBatteryHolderCount(), int.Parse(condis[2]), condis[1]);
				break;
			case "ports":
                switch (condis[1])
                {
					case "duplicates":
						isTrue = bomb.IsDuplicatePortPresent();
						break;
					case "empty":
						isTrue = bomb.GetPortPlates().Any(pp => pp.IsNullOrEmpty());
						break;
					case "any":
						isTrue = Comparer.Compare(bomb.GetPortCount(), int.Parse(condis[3]), condis[2]);
						break;
					case "onone":
						isTrue = bomb.GetPortPlates().Any(pp => condis[2].Split("|").All(port => pp.Contains(port)));
						break;
					default:
						isTrue = Comparer.Compare(bomb.GetPorts().Where(p => condis[1].Split("|").Contains(p)).Count(), int.Parse(condis[3]), condis[2]) ;
						break;
				}
				break;
			case "friend":
				if (stageNumber == 0) isTrue = false;
				else isTrue = Enumerable.Range(0, slotStates.GetLength(0)).Select(x => slotStates[stageNumber - 1, x]).ToArray().Any(s=>s.CharacterName==(CharacterName)Enum.Parse(typeof(CharacterName),condis[1]));
				break;
			case "time":
                switch (condis[1])
                {
					case "left":
						isTrue = Comparer.Compare(Math.Floor(bomb.GetTime()), int.Parse(condis[3]), condis[2]);
						break;
					case "used":
						if (Game.Mission.GeneratorSetting.TimeLimit==0) break;
						isTrue=Comparer.Compare((double)(1-((double)bomb.GetTime() / Game.Mission.GeneratorSetting.TimeLimit))*100,int.Parse(condis[3]),"e"+condis[2]);
						break;
					default:
						throw new ArgumentException(condis[1]+" is not a valid argument.");
                }
				break;
			case "voltage":
				if (bomb.QueryWidgets("volt", "").Count()!=0) isTrue=Comparer.Compare(double.Parse(bomb.QueryWidgets("volt", "")[0].Substring(12).Replace("\"}", "")),int.Parse(condis[2]),condis[1]);
				break;
			case "2factor":
				isTrue = bomb.GetTwoFactorCounts() != 0;
				break;
			case "first":
				isTrue = true;
				int lookUntil = 3;
				for (int i = 0; i <= stageNumber; i++)
                {
					if (slotStates[i, position] == c) lookUntil = position;
					for(int j = 0; j < lookUntil; j++)
						if (slotStates[i, j].CharacterName == c.CharacterName) isTrue = false;
					if (slotStates[i, position] == c) break;
				}
				break;
			case "compare":
				string[] operands = new string[] { condis[1], condis[2] };
				int[] operandValues = new int[2];
				for(int i = 0; i < 2; i++)
                {
					operandValues[i] = Comparer.GetEdgeworkNumber(operands[i], bomb);
                }
				isTrue = Comparer.Compare(operandValues[0], operandValues[1], "more");
				break;
			case "widgetVanilla":
				isTrue = Comparer.Compare(bomb.GetBatteryHolderCount() + bomb.GetIndicators().Count() + bomb.GetPortPlateCount(), int.Parse(condis[2]), condis[1]);
				break;
			case "widgetSum":
				string[] widgets = condis[1].Split("|");
				int sum = 0;
				foreach(string widget in widgets)
					sum += Comparer.GetEdgeworkNumber(widget, bomb);
				isTrue = Comparer.Compare(sum, int.Parse(condis[3]), condis[2]);
				break;
			case "batteries":
                if (condis[1].Equals("compare"))
                {
					if (condis[2].Equals("D")) condis[2] = "more";
					else if (condis[2].Equals("AA")) condis[2] = "less";
					isTrue = Comparer.Compare(bomb.GetBatteryCount(Battery.D), bomb.GetBatteryCount(Battery.AA), condis[2]);
                }
                else
                {
					if (condis[1].Equals("all")) isTrue = Comparer.Compare(bomb.GetBatteryCount(), int.Parse(condis[3]), condis[2]);
					else isTrue = Comparer.Compare(bomb.GetBatteryCount((Battery)Enum.Parse(typeof(Battery),condis[1])), int.Parse(condis[3]), condis[2]);
				}
				break;
			default:
				throw new ArgumentException(condis[0] + " is not a valid instruction.");
        }
		if (condis[condis.Length - 1].Equals("not")) isTrue = !isTrue;
		return isTrue;
    }

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use [!{0} keep 1 2 3] to toggle a slot's keep status. Use [!{0} recall (1 or 2)] to recall a previous stage. Type [crank] at the end of your command (or just [!{0} crank]) to submit your answers.";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
        if (!IsSolved)
        {
			bool validInput = false;
			List<KMSelectable> pressThese = new List<KMSelectable>();
			string[] commandSplit = command.ToLowerInvariant().Split();
			if (commandSplit[0].Equals("recall") && commandSplit.Length==2 && (commandSplit[1].Equals("1") || commandSplit[1].Equals("2")))
			{
				validInput = true;
				pressThese.Add(stageButtons[int.Parse(commandSplit[1]) - 1]);
			}
			else if (commandSplit[0].Equals("keep") && commandSplit.Skip(1).All(i => Enumerable.Range(1, 3).Select(n => n.ToString()).Any(ns=>ns.Equals(i)) || ((i.Equals("crank")) && commandSplit.Skip(1).SkipLast(1).All(n=>!n.Equals("crank")) && commandSplit.Length!=2) ))
			{
				validInput = true;
				foreach (string keeper in commandSplit.Skip(1))
				{
					int i;
					if(int.TryParse(keeper, out i))
                    {
						pressThese.Add(keepButtons[int.Parse(keeper) - 1]);
					}
				}

			}
			else if (commandSplit.Last().Equals("crank"))
			{
				validInput = true;
				pressThese.Add(crank);
			}
            if (validInput)
            {
				yield return null;
				yield return new WaitUntil(() => userInputPossible);
				foreach (KMSelectable button in pressThese)
                {
					button.OnInteract();
					yield return new WaitForSeconds(.10f);
                }
            }
		}
	}

	private IEnumerator TwitchHandleForcedSolve() {
		Log("I am {0} and I am starting my autosolver", Id);
		yield return new WaitUntil(() => userInputPossible);
		while (!IsSolved)
        {
			List<KMSelectable> pressThese = new List<KMSelectable>();
			if (slotStates[stageNumber, 0].CharacterName == slotStates[stageNumber, 1].CharacterName && slotStates[stageNumber, 0].CharacterName == slotStates[stageNumber, 2].CharacterName)
				foreach(int slot in Enumerable.Range(0, 3))
                {
					if (!keepStates[slot]) pressThese.Add(keepButtons[slot]);
				}
			else foreach (int slot in Enumerable.Range(0, 3))
			{
				if (keepStates[slot] != ValidityCheck(slot, true)) pressThese.Add(keepButtons[slot]);
			}
			pressThese.Add(crank);
			foreach(KMSelectable butt in pressThese)
            {
				butt.OnInteract();
				yield return new WaitUntil(() => userInputPossible);
			}
		}
	}
}

